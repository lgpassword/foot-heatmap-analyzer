using FootHeatmapAnalyzer.Web.Models;
using FootHeatmapAnalyzer.Web.Services;

namespace FootHeatmapAnalyzer.Tests;

public sealed class FootAnalysisServiceTests
{
    [Fact]
    public void Analyze_ReturnsExpectedScreeningSections()
    {
        var parser = new FootScanParser();
        var service = new FootAnalysisService();
        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());

        var report = service.Analyze(scan);

        Assert.False(string.IsNullOrWhiteSpace(report.ArchType));
        Assert.False(string.IsNullOrWhiteSpace(report.GaitCycle));
        Assert.False(string.IsNullOrWhiteSpace(report.CenterOfPressure));
        Assert.Contains(report.Findings, finding => finding.Category.Contains("Diabetic"));
        Assert.Contains("non-diagnostic", report.Disclaimer);
    }
}
