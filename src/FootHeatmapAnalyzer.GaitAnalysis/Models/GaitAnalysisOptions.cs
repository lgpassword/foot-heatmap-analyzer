namespace FootHeatmapAnalyzer.GaitAnalysis.Models;

/// <summary>
/// Configures the ONNX model contract used by cloud-side gait analysis.
/// </summary>
public sealed class GaitAnalysisOptions
{
    /// <summary>
    /// Absolute or application-relative path to the ONNX model file.
    /// </summary>
    public string? ModelPath { get; set; }

    /// <summary>
    /// Name of the model input tensor that receives pressure sequence features.
    /// </summary>
    public string InputName { get; set; } = "pressure_sequence";

    /// <summary>
    /// Name of the model output tensor that returns gait class probabilities.
    /// </summary>
    public string OutputName { get; set; } = "probabilities";

    /// <summary>
    /// Ordered class labels corresponding to model output indices.
    /// </summary>
    public string[] Labels { get; set; } = ["Normal", "Pronation", "Supination", "Antalgic"];

    /// <summary>
    /// 配置文件中的类别标签别名，便于 appsettings 使用更直观的键名。
    /// </summary>
    public string[] ClassLabels
    {
        get => Labels;
        set => Labels = value;
    }

    /// <summary>
    /// Expected feature count for demo and validation metadata.
    /// </summary>
    public int InputFeatureCount { get; set; } = 16;
}
