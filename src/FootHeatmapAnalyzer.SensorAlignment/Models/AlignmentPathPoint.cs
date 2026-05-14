namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// 表示 DTW 对齐路径中的一对压力样本索引和加速度样本索引。
/// </summary>
public sealed record AlignmentPathPoint(int PressureIndex, int AccelerometerIndex);
