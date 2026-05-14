namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// Represents one timestamped pressure summary sample.
/// </summary>
public sealed record PressureSample(TimeSpan Timestamp, double Load);
