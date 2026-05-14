namespace FootHeatmapAnalyzer.GaitAnalysis.Models;

/// <summary>
/// Represents one timestamped pressure feature vector in a gait sequence.
/// </summary>
public sealed record PressureSequenceFrame(TimeSpan Timestamp, float[] Features);
