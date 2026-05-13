using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

namespace FootHeatmapAnalyzer.Algorithms.Services;

/// <summary>
/// Converts heatmap pixels into region loads, contact area, hotspots, and center-of-pressure metrics.
/// </summary>
public sealed class HeatmapFeatureExtractor : IHeatmapFeatureExtractor
{
    private const double ContactThreshold = .18;
    private const double HotspotThreshold = .88;

    public FootScanMetrics Extract(ParsedFootScan scan)
    {
        return new FootScanMetrics(ExtractFoot(scan.LeftFoot), ExtractFoot(scan.RightFoot));
    }

    private static FootRegionMetrics ExtractFoot(FootHeatmap foot)
    {
        var forefoot = RegionAverage(foot, 0, .35);
        var midfoot = RegionAverage(foot, .35, .70);
        var heel = RegionAverage(foot, .70, 1);
        var allValues = foot.AllValues().ToArray();
        var totalLoad = allValues.Sum();
        var activeCells = allValues.Count(value => value >= ContactThreshold);
        var center = CalculateCenterOfPressure(foot, totalLoad);
        var archIndex = midfoot / Math.Max(forefoot + midfoot + heel, .01);

        return new FootRegionMetrics(
            foot.Side,
            forefoot,
            midfoot,
            heel,
            totalLoad,
            allValues.DefaultIfEmpty(0).Average(),
            allValues.DefaultIfEmpty(0).Max(),
            activeCells / (double)Math.Max(allValues.Length, 1),
            center.X,
            center.Y,
            allValues.Count(value => value >= HotspotThreshold),
            archIndex);
    }

    private static (double X, double Y) CalculateCenterOfPressure(FootHeatmap foot, double totalLoad)
    {
        if (totalLoad <= 0)
        {
            return (.5, .5);
        }

        var weightedX = 0d;
        var weightedY = 0d;
        for (var y = 0; y < foot.Height; y++)
        {
            for (var x = 0; x < foot.Width; x++)
            {
                var value = foot.Values[y][x];
                weightedX += x * value;
                weightedY += y * value;
            }
        }

        return (
            weightedX / totalLoad / Math.Max(foot.Width - 1, 1),
            weightedY / totalLoad / Math.Max(foot.Height - 1, 1));
    }

    private static double RegionAverage(FootHeatmap foot, double startRatio, double endRatio)
    {
        var start = Math.Clamp((int)Math.Floor(foot.Height * startRatio), 0, foot.Height - 1);
        var end = Math.Clamp((int)Math.Ceiling(foot.Height * endRatio), start + 1, foot.Height);
        var values = foot.Values.Skip(start).Take(end - start).SelectMany(row => row);
        return values.DefaultIfEmpty(0).Average();
    }
}
