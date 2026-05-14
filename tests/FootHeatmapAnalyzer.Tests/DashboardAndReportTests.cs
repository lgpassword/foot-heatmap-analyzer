using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.Web.Dashboard;
using FootHeatmapAnalyzer.Web.Reports;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers dashboard payloads and PDF report generation.
/// </summary>
public sealed class DashboardAndReportTests
{
    [Fact]
    public void Build_ReturnsEChartsReadyMetrics()
    {
        var scan = new FootScanParser().ParseBytes(ParsedFootScan.CreateSampleBytes());
        var metrics = new HeatmapFeatureExtractor().Extract(scan);

        var dashboard = new DashboardService().Build(metrics);

        Assert.Equal(4, dashboard.CenterOfPressureOffset.Values.Length);
        Assert.Equal(3, dashboard.Cadence.Values.Length);
        Assert.InRange(dashboard.LoadBalance.LeftPercent, 0, 100);
        Assert.InRange(dashboard.LoadBalance.RightPercent, 0, 100);
    }

    [Fact]
    public void Generate_ReturnsPdfBytes()
    {
        var parser = new FootScanParser();
        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());
        var service = new FootAnalysisService(new HeatmapFeatureExtractor(), new FootRiskClassifier());
        var metrics = new HeatmapFeatureExtractor().Extract(scan);
        var dashboard = new DashboardService().Build(metrics);

        var pdf = new QuestPdfReportGenerator().Generate(service.Analyze(scan), dashboard);

        Assert.True(pdf.Length > 100);
        Assert.Equal((byte)'%', pdf[0]);
        Assert.Equal((byte)'P', pdf[1]);
    }
}
