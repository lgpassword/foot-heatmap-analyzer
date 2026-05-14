using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// 覆盖未配置 ONNX 模型时的步态分析占位行为。
/// </summary>
public sealed class OnnxGaitServiceTests
{
    [Fact]
    public void Analyze_WhenModelNotConfigured_ReturnsPlaceholderResult()
    {
        var result = CreateService().Predict(CreateSequence());

        Assert.Equal("ModelNotConfigured", result.Label);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void Analyze_WhenModelNotConfigured_DoesNotThrow()
    {
        var exception = Record.Exception(() => CreateService().Predict(CreateSequence()));

        Assert.Null(exception);
    }

    [Fact]
    public void Analyze_PlaceholderResult_HasIsPlaceholderTrue()
    {
        var result = CreateService().Predict(CreateSequence());

        Assert.True(result.IsPlaceholder);
        Assert.Contains("GaitAnalysis:ModelPath", result.Message);
    }

    private static OnnxGaitAnalysisService CreateService()
    {
        return new OnnxGaitAnalysisService(new GaitAnalysisOptions());
    }

    private static IReadOnlyList<PressureSequenceFrame> CreateSequence()
    {
        return
        [
            new PressureSequenceFrame(TimeSpan.Zero, [.1f, .2f, .3f, .4f]),
            new PressureSequenceFrame(TimeSpan.FromMilliseconds(20), [.2f, .3f, .4f, .5f])
        ];
    }
}
