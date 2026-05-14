namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// 表示左右足各一个同类型指标值。
/// </summary>
public sealed record SidePair<T>(T Left, T Right);
