using FootHeatmapAnalyzer.Web.Models;
using FootHeatmapAnalyzer.Web.Services;

namespace FootHeatmapAnalyzer.Tests;

public sealed class FootAnalysisServiceTests
{
    [Fact]
    public void Analyze_ReturnsExpectedScreeningSections()
    {
        var parser = new FootScanParser();
        var service = new FootAnalysisService(new HeatmapFeatureExtractor(), new FootRiskClassifier());
        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());

        var report = service.Analyze(scan);

        Assert.False(string.IsNullOrWhiteSpace(report.ArchType));
        Assert.False(string.IsNullOrWhiteSpace(report.GaitCycle));
        Assert.False(string.IsNullOrWhiteSpace(report.CenterOfPressure));
        Assert.Contains(report.Findings, finding => finding.Category.Contains("Diabetic"));
        Assert.Contains(report.Findings, finding => finding.Category.Contains("recognition"));
        Assert.Contains("non-diagnostic", report.Disclaimer);
    }

    [Fact]
    public void Extract_ReturnsRecognitionFeaturesForBothFeet()
    {
        var parser = new FootScanParser();
        var extractor = new HeatmapFeatureExtractor();
        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());

        var metrics = extractor.Extract(scan);

        Assert.InRange(metrics.CombinedArchIndex, 0, 1);
        Assert.InRange(metrics.LeftLoadShare, 0, 1);
        Assert.InRange(metrics.Left.CenterX, 0, 1);
        Assert.InRange(metrics.Right.CenterY, 0, 1);
        Assert.True(metrics.ContactAreaRatio > 0);
    }

    [Fact]
    public void ClassifyArch_UsesArchIndexBand()
    {
        var classifier = new FootRiskClassifier();
        var left = new FootRegionMetrics(FootSide.Left, .6, .2, .6, 10, .4, .8, .4, .5, .5, 0, .22);
        var right = new FootRegionMetrics(FootSide.Right, .3, .3, .3, 10, .4, .8, .7, .5, .5, 0, .28);

        var report = classifier.ClassifyArch(new FootScanMetrics(left, right));

        Assert.Equal("Neutral arch tendency", report);
    }
}
