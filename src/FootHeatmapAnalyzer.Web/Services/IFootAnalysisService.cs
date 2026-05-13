using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Produces non-diagnostic auxiliary findings from parsed foot heatmaps.
/// </summary>
public interface IFootAnalysisService
{
    FootAnalysisReport Analyze(ParsedFootScan scan);
}
