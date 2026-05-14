using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Orchestrates heatmap feature extraction and non-diagnostic report generation.
/// </summary>
public sealed class FootAnalysisService(IHeatmapFeatureExtractor featureExtractor, IFootRiskClassifier classifier) : IFootAnalysisService
{
    private const string Disclaimer = "本项目仅提供面向筛查和研究演示的非诊断性结果，不用于疾病诊断，也不能替代临床评估。";

    public FootAnalysisReport Analyze(ParsedFootScan scan)
    {
        var metrics = featureExtractor.Extract(scan);
        return classifier.BuildReport(metrics, Disclaimer);
    }
}
