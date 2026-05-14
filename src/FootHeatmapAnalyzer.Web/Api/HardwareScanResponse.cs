using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Web.Dashboard;

namespace FootHeatmapAnalyzer.Web.Api;

/// <summary>
/// Returns normalized analysis data for an ingested hardware scan.
/// </summary>
public sealed record HardwareScanResponse(
    string ScanId,
    string DeviceId,
    string TenantId,
    string? ProfileId,
    FootAnalysisReport Report,
    HeatmapRenderFrame RenderFrame,
    DashboardPayload Dashboard);
