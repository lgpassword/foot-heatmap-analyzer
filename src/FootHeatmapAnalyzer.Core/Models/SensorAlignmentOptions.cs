namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// 配置传感器对齐算法在序列长度差异较大时的重采样策略。
/// </summary>
public sealed class SensorAlignmentOptions
{
    /// <summary>
    /// 两组序列长度超过该倍率时，可先重采样再进行 DTW。
    /// </summary>
    public double MaxSeriesLengthRatio { get; set; } = 3.0;

    /// <summary>
    /// 重采样后的目标点数。
    /// </summary>
    public int ResampleTargetCount { get; set; } = 100;
}
