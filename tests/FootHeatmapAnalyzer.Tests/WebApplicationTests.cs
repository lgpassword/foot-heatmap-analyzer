using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.SensorAlignment.Models;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers the Razor host and browser-facing security contracts.
/// </summary>
public sealed class WebApplicationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// Creates the test fixture using the real web entry point.
    /// </summary>
    public WebApplicationTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task GetHome_ReturnsHeatmapDataBlock()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("""<script type="application/json" id="heatmap-data">""", html);
        Assert.Contains("id=\"combinedHeatmap\"", html);
        Assert.Contains("\"left\"", html);
        Assert.DoesNotContain("data-heatmap", html);
        Assert.DoesNotContain("id=\"leftHeatmap\"", html);
        Assert.DoesNotContain("id=\"rightHeatmap\"", html);
        Assert.DoesNotContain("id=\"loadSample\"", html);
    }

    [Fact]
    public async Task GetHome_AddsSecurityHeaders()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Contains("connect-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task PostAnalyze_ReturnsReportForBinaryPayload()
    {
        var client = factory.CreateClient();
        using var content = new ByteArrayContent(ParsedFootScan.CreateSampleBytes());
        content.Headers.ContentType = new("application/octet-stream");

        var response = await client.PostAsync("/api/analyze", content);
        var report = await response.Content.ReadFromJsonAsync<FootAnalysisReport>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(report);
        Assert.False(string.IsNullOrWhiteSpace(report.ArchType));
        Assert.Contains(report.Findings, finding => finding.Category.Contains("局部高压热点"));
    }

    [Fact]
    public async Task PostAnalyzeText_ReturnsReportForHexPayload()
    {
        var client = factory.CreateClient();
        var hex = Convert.ToHexString(ParsedFootScan.CreateSampleBytes());
        using var content = new StringContent(hex, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/analyze/text", content);
        var report = await response.Content.ReadFromJsonAsync<FootAnalysisReport>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(report);
        Assert.False(string.IsNullOrWhiteSpace(report.ArchType));
    }

    [Fact]
    public async Task PostAnalyzeText_ReturnsBadRequestForInvalidPayload()
    {
        var client = factory.CreateClient();
        using var content = new StringContent("invalid", Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/analyze/text", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostRenderFrameText_ReturnsCompressedHeatmapPayload()
    {
        var client = factory.CreateClient();
        var hex = Convert.ToHexString(ParsedFootScan.CreateSampleBytes());
        using var content = new StringContent(hex, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/render-frame/text", content);
        var frame = await response.Content.ReadFromJsonAsync<HeatmapRenderFrame>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(frame);
        Assert.Equal("bicubic", frame.Interpolation);
        Assert.False(string.IsNullOrWhiteSpace(frame.Left.Data));
    }

    [Fact]
    public async Task GetHeatmapHubEndpoint_ExistsForSignalRNegotiation()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/hubs/heatmap/negotiate?negotiateVersion=1", null);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PostGaitAnalyze_ReturnsBadRequestWhenModelIsNotConfigured()
    {
        var client = factory.CreateClient();
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [1f, .5f])
        };

        var response = await client.PostAsJsonAsync("/api/gait/analyze", sequence);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostSensorsAlign_ReturnsDtwPath()
    {
        var client = factory.CreateClient();
        var request = new SensorAlignmentRequest(
            [
                new PressureSample(TimeSpan.Zero, 1),
                new PressureSample(TimeSpan.FromMilliseconds(20), 2)
            ],
            [
                new AccelerometerSample(TimeSpan.Zero, 1, 0, 0),
                new AccelerometerSample(TimeSpan.FromMilliseconds(10), 2, 0, 0)
            ]);

        var response = await client.PostAsJsonAsync("/api/sensors/align", request);
        var result = await response.Content.ReadFromJsonAsync<SensorAlignmentResult>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Path);
    }
}
