namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Contains the parsed left and right foot heatmaps from one scan payload.
/// </summary>
public sealed record ParsedFootScan(FootHeatmap LeftFoot, FootHeatmap RightFoot)
{
    /// <summary>
    /// Creates a deterministic sample payload for demos and tests.
    /// </summary>
    public static byte[] CreateSampleBytes()
    {
        const int width = 8;
        const int height = 12;
        var bytes = new List<byte> { width, height };

        for (var foot = 0; foot < 2; foot++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var archRelief = x is >= 3 and <= 4 && y is >= 4 and <= 7 ? 45 : 0;
                    var heel = y > 8 ? 85 : 0;
                    var forefoot = y < 4 ? 95 : 0;
                    var lateralBias = foot == 0 ? x * 4 : (width - x) * 4;
                    bytes.Add((byte)Math.Clamp(35 + heel + forefoot + lateralBias - archRelief, 0, 255));
                }
            }
        }

        return bytes.ToArray();
    }
}
