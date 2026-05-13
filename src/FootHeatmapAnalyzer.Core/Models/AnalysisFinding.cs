namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Describes one non-diagnostic screening signal derived from a heatmap.
/// </summary>
public sealed record AnalysisFinding(string Category, string Level, string Summary, string Rationale);
