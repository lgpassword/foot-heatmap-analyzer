namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// Represents one named chart series for browser dashboard libraries.
/// </summary>
public sealed record ChartSeries(string Title, string[] Labels, double[] Values);
