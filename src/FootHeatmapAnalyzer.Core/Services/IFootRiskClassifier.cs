using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Converts extracted heatmap metrics into non-diagnostic screening output.
/// </summary>
public interface IFootRiskClassifier
{
    string ClassifyArch(FootScanMetrics metrics);

    string DescribeGait(FootScanMetrics metrics);

    string DescribeBalance(FootScanMetrics metrics);

    IReadOnlyList<AnalysisFinding> BuildFindings(FootScanMetrics metrics, string balance);
}
