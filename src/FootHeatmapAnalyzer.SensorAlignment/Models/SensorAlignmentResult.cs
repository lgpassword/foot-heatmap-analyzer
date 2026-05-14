namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// Contains the DTW path and total alignment cost for non-uniform sensor streams.
/// </summary>
public sealed record SensorAlignmentResult(double Cost, IReadOnlyList<AlignedSensorSample> Path);
