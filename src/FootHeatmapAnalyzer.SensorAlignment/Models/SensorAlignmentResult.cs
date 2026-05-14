namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// Contains the DTW path and total alignment cost for non-uniform sensor streams.
/// </summary>
public sealed record SensorAlignmentResult(double Cost, IReadOnlyList<AlignedSensorSample> Path)
{
    /// <summary>
    /// DTW 对齐路径的索引序列，便于开放 API 直接返回轻量 JSON。
    /// </summary>
    public IReadOnlyList<AlignmentPathPoint> AlignmentPath { get; init; } =
        Path.Select(sample => new AlignmentPathPoint(sample.PressureIndex, sample.AccelerometerIndex)).ToArray();

    /// <summary>
    /// 按路径长度归一化后的 DTW 距离。
    /// </summary>
    public double NormalizedDistance { get; init; } = Path.Count == 0 ? 0 : Cost / Path.Count;
}
