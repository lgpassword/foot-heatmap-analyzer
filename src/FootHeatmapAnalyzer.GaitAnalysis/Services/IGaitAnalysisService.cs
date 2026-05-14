using FootHeatmapAnalyzer.GaitAnalysis.Models;

namespace FootHeatmapAnalyzer.GaitAnalysis.Services;

/// <summary>
/// Runs cloud-side gait recognition on timestamped pressure sequences.
/// </summary>
public interface IGaitAnalysisService
{
    /// <summary>
    /// Predicts the most likely gait class for an uploaded pressure sequence.
    /// </summary>
    GaitPrediction Predict(IReadOnlyList<PressureSequenceFrame> sequence);
}
