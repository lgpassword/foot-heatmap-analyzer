namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Contains compact raw pressure arrays and metadata used by GPU/WebGL browser renderers.
/// </summary>
public sealed record HeatmapRenderFrame(
    string FrameId,
    DateTimeOffset CapturedAt,
    CompressedFootHeatmap Left,
    CompressedFootHeatmap Right,
    string Interpolation,
    string Transport)
{
    /// <summary>
    /// Creates a render frame from one parsed scan.
    /// </summary>
    public static HeatmapRenderFrame FromScan(ParsedFootScan scan, DateTimeOffset capturedAt)
    {
        ArgumentNullException.ThrowIfNull(scan);

        return new HeatmapRenderFrame(
            Convert.ToHexString(Guid.NewGuid().ToByteArray()),
            capturedAt,
            CompressedFootHeatmap.FromHeatmap(scan.LeftFoot),
            CompressedFootHeatmap.FromHeatmap(scan.RightFoot),
            "bicubic",
            "raw-pressure-base64");
    }
}
