using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Produces non-diagnostic auxiliary findings from parsed foot heatmaps.
/// </summary>
public interface IFootAnalysisService
{
    FootAnalysisReport Analyze(ParsedFootScan scan);
}
