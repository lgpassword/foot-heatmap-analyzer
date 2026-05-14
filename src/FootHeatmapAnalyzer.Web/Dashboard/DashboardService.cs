using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Web.Dashboard;

/// <summary>
/// Creates CoP, cadence, and load-balance dashboard data from extracted metrics.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    /// <inheritdoc />
    public DashboardPayload Build(FootScanMetrics metrics)
    {
        var copValues = new[]
        {
            Math.Round((metrics.Left.CenterX - .5) * 100, 2),
            Math.Round((metrics.Left.CenterY - .5) * 100, 2),
            Math.Round((metrics.Right.CenterX - .5) * 100, 2),
            Math.Round((metrics.Right.CenterY - .5) * 100, 2)
        };
        var averageForefoot = (metrics.Left.ForefootLoad + metrics.Right.ForefootLoad) / 2;
        var averageHeel = (metrics.Left.HeelLoad + metrics.Right.HeelLoad) / 2;
        var cadenceProxy = new[]
        {
            Math.Round(averageHeel * 120, 2),
            Math.Round(((averageHeel + averageForefoot) / 2) * 120, 2),
            Math.Round(averageForefoot * 120, 2)
        };
        var leftPercent = Math.Round(metrics.LeftLoadShare * 100, 1);
        var rightPercent = Math.Round(100 - leftPercent, 1);
        var leftForefootPct = PercentOfLoad(metrics.Left.ForefootLoad, metrics.Left.ForefootLoad + metrics.Left.HeelLoad);
        var rightForefootPct = PercentOfLoad(metrics.Right.ForefootLoad, metrics.Right.ForefootLoad + metrics.Right.HeelLoad);
        var leftHeelPct = Math.Round(100 - leftForefootPct, 1);
        var rightHeelPct = Math.Round(100 - rightForefootPct, 1);
        var copX = Math.Round(((metrics.Left.CenterX + (1 - metrics.Right.CenterX)) / 2) - .5, 3);
        var copY = Math.Round(((metrics.Left.CenterY + metrics.Right.CenterY) / 2) - .5, 3);

        return new DashboardPayload(
            new ChartSeries("压力中心偏移", ["左X", "左Y", "右X", "右Y"], copValues),
            new ChartSeries("步频代理", ["足跟", "过渡", "前足"], cadenceProxy),
            new GaugeMetric("左右受力平衡", leftPercent, rightPercent),
            new EChartsOption(
                new { data = new[] { "Left", "Right" } },
                new { type = "value" },
                [new { type = "bar", data = new[] { leftPercent, rightPercent } }]),
            new EChartsOption(
                new { min = -1, max = 1 },
                new { min = -1, max = 1 },
                [new { type = "scatter", data = new[] { new[] { copX, copY } } }]),
            new EChartsOption(
                new { data = new[] { "Left", "Right" } },
                new { type = "value" },
                [
                    new { name = "Forefoot", type = "bar", stack = "total", data = new[] { leftForefootPct, rightForefootPct } },
                    new { name = "Heel", type = "bar", stack = "total", data = new[] { leftHeelPct, rightHeelPct } }
                ]),
            new EChartsOption(
                null,
                null,
                [new
                {
                    type = "pie",
                    data = new[]
                    {
                        new { name = "Left Hotspots", value = metrics.Left.HotspotCount },
                        new { name = "Right Hotspots", value = metrics.Right.HotspotCount }
                    }
                }]));
    }

    private static double PercentOfLoad(double value, double total)
    {
        return Math.Round((value / Math.Max(total, .001)) * 100, 1);
    }
}
