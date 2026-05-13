using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Extracts normalized recognition features from parsed heatmaps.
/// </summary>
public interface IHeatmapFeatureExtractor
{
    FootScanMetrics Extract(ParsedFootScan scan);
}
