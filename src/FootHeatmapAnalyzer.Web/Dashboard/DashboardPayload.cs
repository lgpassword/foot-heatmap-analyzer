namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// Contains ECharts-ready chart data for pressure analysis dashboards.
/// </summary>
public sealed record DashboardPayload(
    ChartSeries CenterOfPressureOffset,
    ChartSeries Cadence,
    GaugeMetric LoadBalance,
    EChartsOption BalanceChart,
    EChartsOption CopChart,
    EChartsOption LoadDistChart,
    EChartsOption HotspotChart);
