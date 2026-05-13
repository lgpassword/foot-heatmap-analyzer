using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Orchestrates heatmap feature extraction and non-diagnostic report generation.
/// </summary>
public sealed class FootAnalysisService(IHeatmapFeatureExtractor featureExtractor, FootRiskClassifier classifier) : IFootAnalysisService
{
    private const string Disclaimer = "This project provides screening-oriented, non-diagnostic research output. It does not diagnose disease or replace clinical evaluation.";

    public FootAnalysisReport Analyze(ParsedFootScan scan)
    {
        var metrics = featureExtractor.Extract(scan);
        var archType = classifier.ClassifyArch(metrics);
        var balance = classifier.DescribeBalance(metrics);
        var gait = classifier.DescribeGait(metrics);
        var findings = classifier.BuildFindings(metrics, balance);

        return new FootAnalysisReport(archType, gait, balance, findings, Disclaimer);
    }
}
