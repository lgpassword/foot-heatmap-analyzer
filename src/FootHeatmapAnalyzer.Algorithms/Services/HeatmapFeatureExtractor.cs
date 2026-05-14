using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

namespace FootHeatmapAnalyzer.Algorithms.Services;

/// <summary>
/// Converts heatmap pixels into region loads, contact area, hotspots, and center-of-pressure metrics.
/// </summary>
public sealed class HeatmapFeatureExtractor(AnalysisOptions? options = null) : IHeatmapFeatureExtractor
{
    // Holds thresholds used while converting raw cells into aggregate features.
    private readonly AnalysisOptions options = options ?? new AnalysisOptions();

    public FootScanMetrics Extract(ParsedFootScan scan)
    {
        return new FootScanMetrics(ExtractFoot(scan.LeftFoot), ExtractFoot(scan.RightFoot));
    }

    private FootRegionMetrics ExtractFoot(FootHeatmap foot)
    {
        var totalLoad = 0d;
        var peakPressure = 0d;
        var weightedX = 0d;
        var weightedY = 0d;
        var forefootLoad = 0d;
        var midfootLoad = 0d;
        var heelLoad = 0d;
        var activeCells = 0;
        var hotspotCount = 0;
        var forefootCells = 0;
        var midfootCells = 0;
        var heelCells = 0;
        var forefootEnd = (int)Math.Ceiling(foot.Height * options.ForefootRegionEndRatio);
        var midfootEnd = (int)Math.Ceiling(foot.Height * options.MidfootRegionEndRatio);

        for (var y = 0; y < foot.Height; y++)
        {
            for (var x = 0; x < foot.Width; x++)
            {
                var value = foot.At(x, y);
                totalLoad += value;
                peakPressure = Math.Max(peakPressure, value);
                weightedX += x * value;
                weightedY += y * value;

                if (value >= options.ContactThreshold)
                {
                    activeCells++;
                }

                if (value >= options.HotspotThreshold)
                {
                    hotspotCount++;
                }

                if (y < forefootEnd)
                {
                    forefootLoad += value;
                    forefootCells++;
                }
                else if (y < midfootEnd)
                {
                    midfootLoad += value;
                    midfootCells++;
                }
                else
                {
                    heelLoad += value;
                    heelCells++;
                }
            }
        }

        var cellCount = Math.Max(foot.Width * foot.Height, 1);
        var forefoot = forefootLoad / Math.Max(forefootCells, 1);
        var midfoot = midfootLoad / Math.Max(midfootCells, 1);
        var heel = heelLoad / Math.Max(heelCells, 1);
        var centerX = totalLoad <= 0 ? .5 : weightedX / totalLoad / Math.Max(foot.Width - 1, 1);
        var centerY = totalLoad <= 0 ? .5 : weightedY / totalLoad / Math.Max(foot.Height - 1, 1);
        var archIndex = midfoot / Math.Max(forefoot + midfoot + heel, options.MinimumArchLoadDenominator);

        return new FootRegionMetrics(
            foot.Side,
            forefoot,
            midfoot,
            heel,
            totalLoad,
            totalLoad / cellCount,
            peakPressure,
            activeCells / (double)cellCount,
            centerX,
            centerY,
            hotspotCount,
            archIndex);
    }
}
