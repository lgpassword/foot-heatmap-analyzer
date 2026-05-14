using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers cloud-side gait analysis service contracts before model assets are provided.
/// </summary>
public sealed class GaitAnalysisTests
{
    [Fact]
    public void Predict_RequiresConfiguredOnnxModel()
    {
        var service = new OnnxGaitAnalysisService(new GaitAnalysisOptions());
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [1f, .5f])
        };

        var error = Assert.Throws<InvalidOperationException>(() => service.Predict(sequence));

        Assert.Contains("ONNX gait model path", error.Message);
    }

    [Fact]
    public void Predict_RejectsInconsistentFeatureVectors()
    {
        var service = new OnnxGaitAnalysisService(new GaitAnalysisOptions { ModelPath = "missing.onnx" });
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [1f, .5f]),
            new PressureSequenceFrame(TimeSpan.FromMilliseconds(10), [1f])
        };

        Assert.Throws<ArgumentException>(() => service.Predict(sequence));
    }
}
