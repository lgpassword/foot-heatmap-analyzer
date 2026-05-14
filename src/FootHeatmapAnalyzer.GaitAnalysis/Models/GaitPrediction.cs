namespace FootHeatmapAnalyzer.GaitAnalysis.Models;

/// <summary>
/// Describes the highest-probability gait class returned by the ONNX model.
/// </summary>
public sealed record GaitPrediction(string Label, float Confidence, IReadOnlyDictionary<string, float> Probabilities);
