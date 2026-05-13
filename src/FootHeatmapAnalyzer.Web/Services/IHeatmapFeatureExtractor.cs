using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Extracts normalized recognition features from parsed heatmaps.
/// </summary>
public interface IHeatmapFeatureExtractor
{
    FootScanMetrics Extract(ParsedFootScan scan);
}
