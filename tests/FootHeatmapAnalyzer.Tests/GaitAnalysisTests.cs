using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers cloud-side gait analysis service contracts.
/// </summary>
public sealed class GaitAnalysisTests
{
    [Fact]
    public void Predict_UsesBuiltInClassifierWhenOnnxModelIsNotConfigured()
    {
        var service = new OnnxGaitAnalysisService(new GaitAnalysisOptions());
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [.8f, .8f, .4f, .4f]),
            new PressureSequenceFrame(TimeSpan.FromMilliseconds(20), [.75f, .78f, .42f, .41f])
        };

        var prediction = service.Predict(sequence);

        Assert.Equal("正常步态", prediction.Label);
        Assert.InRange(prediction.Confidence, 0, 1);
        Assert.Contains("左右不对称", prediction.Probabilities.Keys);
    }

    [Fact]
    public void Predict_RejectsInconsistentFeatureVectors()
    {
        var service = new OnnxGaitAnalysisService(new GaitAnalysisOptions());
        var sequence = new[]
        {
            new PressureSequenceFrame(TimeSpan.Zero, [1f, .5f]),
            new PressureSequenceFrame(TimeSpan.FromMilliseconds(10), [1f])
        };

        Assert.Throws<ArgumentException>(() => service.Predict(sequence));
    }
}
