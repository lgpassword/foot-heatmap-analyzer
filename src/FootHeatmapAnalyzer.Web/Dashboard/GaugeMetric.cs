namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// Represents one percentage gauge for dashboard display.
/// </summary>
public sealed record GaugeMetric(string Title, double LeftPercent, double RightPercent);
