using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.Web.Dashboard;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// 覆盖仪表盘输出给 ECharts 的四类图表配置。
/// </summary>
public sealed class DashboardPayloadTests
{
    [Fact]
    public void Dashboard_ContainsBalanceChart()
    {
        Assert.NotNull(CreateDashboard().BalanceChart);
    }

    [Fact]
    public void Dashboard_ContainsCopChart()
    {
        Assert.NotNull(CreateDashboard().CopChart);
    }

    [Fact]
    public void Dashboard_ContainsLoadDistChart()
    {
        Assert.NotNull(CreateDashboard().LoadDistChart);
    }

    [Fact]
    public void Dashboard_ContainsHotspotChart()
    {
        Assert.NotNull(CreateDashboard().HotspotChart);
    }

    [Fact]
    public void Dashboard_AllValuesAreFinite()
    {
        var dashboard = CreateDashboard();

        Assert.All(dashboard.CenterOfPressureOffset.Values, AssertFinite);
        Assert.All(dashboard.Cadence.Values, AssertFinite);
        AssertFinite(dashboard.LoadBalance.LeftPercent);
        AssertFinite(dashboard.LoadBalance.RightPercent);
    }

    private static DashboardPayload CreateDashboard()
    {
        var scan = new FootScanParser().ParseBytes(ParsedFootScan.CreateSampleBytes());
        var metrics = new HeatmapFeatureExtractor().Extract(scan);
        return new DashboardService().Build(metrics);
    }

    private static void AssertFinite(double value)
    {
        Assert.False(double.IsNaN(value));
        Assert.False(double.IsInfinity(value));
    }
}
