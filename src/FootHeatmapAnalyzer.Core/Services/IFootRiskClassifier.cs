using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Converts extracted heatmap metrics into non-diagnostic screening output.
/// </summary>
public interface IFootRiskClassifier
{
    string ClassifyArch(FootScanMetrics metrics);

    string DescribeGait(FootScanMetrics metrics);

    BalanceResult DescribeBalance(FootScanMetrics metrics);

    IReadOnlyList<AnalysisFinding> BuildFindings(FootScanMetrics metrics, BalanceResult balance);

    /// <summary>
    /// 根据提取后的指标生成完整筛查报告。
    /// </summary>
    FootAnalysisReport BuildReport(FootScanMetrics metrics, string disclaimer);
}
