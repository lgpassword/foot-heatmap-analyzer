namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// 表示可直接传给 ECharts setOption 的轻量配置对象。
/// </summary>
public sealed record EChartsOption(object? XAxis, object? YAxis, IReadOnlyList<object> Series);
