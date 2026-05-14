namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Represents one normalized foot matrix packed as 8-bit raw pressure values for browser-side rendering.
/// </summary>
public sealed record CompressedFootHeatmap(FootSide Side, int Width, int Height, string Encoding, string Data)
{
    /// <summary>
    /// Creates a base64-encoded byte payload from a normalized heatmap.
    /// </summary>
    public static CompressedFootHeatmap FromHeatmap(FootHeatmap heatmap)
    {
        ArgumentNullException.ThrowIfNull(heatmap);

        var bytes = new byte[heatmap.Width * heatmap.Height];
        var values = heatmap.AsSpan();
        for (var index = 0; index < values.Length; index++)
        {
            bytes[index] = (byte)Math.Clamp(Math.Round(values[index] * byte.MaxValue), 0, byte.MaxValue);
        }

        return new CompressedFootHeatmap(
            heatmap.Side,
            heatmap.Width,
            heatmap.Height,
            "base64:u8-normalized",
            Convert.ToBase64String(bytes));
    }
}
