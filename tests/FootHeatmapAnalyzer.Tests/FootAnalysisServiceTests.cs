using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

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
        Assert.Contains(report.Findings, finding => finding.Category.Contains("局部高压热点"));
        Assert.Contains(report.Findings, finding => finding.Category.Contains("识别质量"));
        Assert.Contains("非诊断性", report.Disclaimer);
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

        Assert.Equal("中性足弓倾向", report);
    }

    [Fact]
    public void ClassifyArch_UsesConfiguredArchThresholds()
    {
        var classifier = new FootRiskClassifier(new AnalysisOptions
        {
            HighArchUpperBound = .30,
            LowArchLowerBound = .70
        });
        var left = new FootRegionMetrics(FootSide.Left, .6, .2, .6, 10, .4, .8, .4, .5, .5, 0, .25);
        var right = new FootRegionMetrics(FootSide.Right, .6, .2, .6, 10, .4, .8, .4, .5, .5, 0, .25);

        var report = classifier.ClassifyArch(new FootScanMetrics(left, right));

        Assert.Equal("高足弓倾向", report);
    }

    [Fact]
    public void Extract_UsesConfiguredContactThreshold()
    {
        var extractor = new HeatmapFeatureExtractor(new AnalysisOptions { ContactThreshold = .60 });
        var left = new FootHeatmap(FootSide.Left, 1, 1, [.5]);
        var right = new FootHeatmap(FootSide.Right, 1, 1, [.7]);

        var metrics = extractor.Extract(new ParsedFootScan(left, right));

        Assert.Equal(0, metrics.Left.ContactAreaRatio);
        Assert.Equal(1, metrics.Right.ContactAreaRatio);
    }

    [Theory]
    [InlineData(.40, BalanceState.RightHeavy)]
    [InlineData(.50, BalanceState.Balanced)]
    [InlineData(.60, BalanceState.LeftHeavy)]
    public void DescribeBalance_ReturnsStructuredState(double leftLoadShare, BalanceState expected)
    {
        var classifier = new FootRiskClassifier();
        var left = new FootRegionMetrics(FootSide.Left, .3, .3, .3, leftLoadShare, .4, .8, .5, .5, .5, 0, .25);
        var right = new FootRegionMetrics(FootSide.Right, .3, .3, .3, 1 - leftLoadShare, .4, .8, .5, .5, .5, 0, .25);

        var balance = classifier.DescribeBalance(new FootScanMetrics(left, right));

        Assert.Equal(expected, balance.State);
        Assert.False(string.IsNullOrWhiteSpace(balance.Label));
    }
}
