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
}
