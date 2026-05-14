using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Web.Dashboard;

namespace FootHeatmapAnalyzer.Web.Reports;

/// <summary>
/// Generates printable pressure analysis reports.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Builds a PDF report from analysis and dashboard data.
    /// </summary>
    byte[] Generate(FootAnalysisReport report, DashboardPayload dashboard);

    /// <summary>
    /// Builds a PDF report from analysis, dashboard, scan, and metrics data.
    /// </summary>
    byte[] Generate(FootAnalysisReport report, DashboardPayload dashboard, ParsedFootScan scan, FootScanMetrics metrics, string inputFormat);
}
