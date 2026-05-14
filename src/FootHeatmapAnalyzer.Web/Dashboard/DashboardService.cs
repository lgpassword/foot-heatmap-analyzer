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

        return new DashboardPayload(
            new ChartSeries("CoP offset", ["Left X", "Left Y", "Right X", "Right Y"], copValues),
            new ChartSeries("Cadence proxy", ["Heel strike", "Mid stance", "Toe off"], cadenceProxy),
            new GaugeMetric("Left/right load balance", leftPercent, Math.Round(100 - leftPercent, 1)));
    }
}
