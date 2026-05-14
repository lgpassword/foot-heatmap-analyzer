using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.SensorAlignment.Models;
using FootHeatmapAnalyzer.Web.Api;
using FootHeatmapAnalyzer.Web.Dashboard;
using FootHeatmapAnalyzer.Web.Tenancy;

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
        Assert.Contains("id=\"dashboardCharts\"", html);
        Assert.Contains("id=\"profileList\"", html);
        Assert.Contains("id=\"hardwareApiResult\"", html);
        Assert.Contains("data-app-action=\"download-pdf\"", html);
        Assert.Contains("data-platform-action=\"create-profile\"", html);
        Assert.Contains("data-platform-action=\"submit-hardware\"", html);
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
    public async Task ProfileApi_IsolatesProfilesByTenantHeader()
    {
        var client = factory.CreateClient();
        using var create = new HttpRequestMessage(HttpMethod.Post, "/api/profiles")
        {
            Content = JsonContent.Create(new CreateProfileRequest(ProfileKind.Patient, "Demo Patient", null, null))
        };
        create.Headers.Add("X-Tenant-Id", "clinic-a");

        var created = await client.SendAsync(create);

        created.EnsureSuccessStatusCode();

        using var listA = new HttpRequestMessage(HttpMethod.Get, "/api/profiles");
        listA.Headers.Add("X-Tenant-Id", "clinic-a");
        using var listB = new HttpRequestMessage(HttpMethod.Get, "/api/profiles");
        listB.Headers.Add("X-Tenant-Id", "clinic-b");

        var tenantA = await (await client.SendAsync(listA)).Content.ReadFromJsonAsync<ManagedProfile[]>();
        var tenantB = await (await client.SendAsync(listB)).Content.ReadFromJsonAsync<ManagedProfile[]>();

        Assert.Single(tenantA!);
        Assert.Empty(tenantB!);
    }

    [Fact]
    public async Task HardwareScanApi_ReturnsAnalysisDashboardAndRenderFrame()
    {
        var client = factory.CreateClient();
        var request = new HardwareScanRequest(
            "device-1",
            "tenant-1",
            "profile-1",
            "hex",
            Convert.ToHexString(ParsedFootScan.CreateSampleBytes()),
            DateTimeOffset.UnixEpoch);

        var response = await client.PostAsJsonAsync("/api/hardware/scans", request);
        var result = await response.Content.ReadFromJsonAsync<HardwareScanResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(result);
        Assert.Equal("device-1", result.DeviceId);
        Assert.NotNull(result.Report);
        Assert.False(string.IsNullOrWhiteSpace(result.RenderFrame.Left.Data));
        Assert.NotNull(result.Dashboard);
    }

    [Fact]
    public async Task DashboardApi_ReturnsChartPayload()
    {
        var client = factory.CreateClient();
        using var content = new StringContent(Convert.ToHexString(ParsedFootScan.CreateSampleBytes()), Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/dashboard", content);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardPayload>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.Equal(4, dashboard.CenterOfPressureOffset.Values.Length);
    }

    [Fact]
    public async Task PdfReportApi_ReturnsPdf()
    {
        var client = factory.CreateClient();
        using var content = new StringContent(Convert.ToHexString(ParsedFootScan.CreateSampleBytes()), Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/reports/pdf", content);
        var bytes = await response.Content.ReadAsByteArrayAsync();

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.True(bytes.Length > 100);
    }

    [Fact]
    public async Task PostGaitAnalyze_ReturnsBuiltInPredictionWhenModelIsNotConfigured()
    {
        var client = factory.CreateClient();
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [.8f, .8f, .4f, .4f]),
            new PressureSequenceFrame(TimeSpan.FromMilliseconds(20), [.75f, .78f, .42f, .41f])
        };

        var response = await client.PostAsJsonAsync("/api/gait/analyze", sequence);
        var prediction = await response.Content.ReadFromJsonAsync<GaitPrediction>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(prediction);
        Assert.False(string.IsNullOrWhiteSpace(prediction.Label));
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
        Assert.All(result.Path.Zip(result.Path.Skip(1)), pair =>
        {
            Assert.True(pair.First.PressureIndex <= pair.Second.PressureIndex);
            Assert.True(pair.First.AccelerometerIndex <= pair.Second.AccelerometerIndex);
        });
    }
}
