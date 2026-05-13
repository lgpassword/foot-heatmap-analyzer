using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Converts a binary foot scan payload into left and right heatmap matrices.
/// </summary>
public interface IFootScanParser
{
    ParsedFootScan ParseBytes(byte[] payload);

    Task<ParsedFootScan> ParseFileAsync(IFormFile file, CancellationToken cancellationToken = default);

    ParsedFootScan ParseText(string input);
}
