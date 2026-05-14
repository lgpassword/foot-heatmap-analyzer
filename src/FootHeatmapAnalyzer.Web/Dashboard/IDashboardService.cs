using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// Builds ECharts-ready dashboard data from analysis metrics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Converts one analysis scan into dashboard chart payloads.
    /// </summary>
    DashboardPayload Build(FootScanMetrics metrics);
}
