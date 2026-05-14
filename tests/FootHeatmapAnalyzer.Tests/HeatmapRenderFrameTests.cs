using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers compact raw heatmap frames used by browser-side GPU renderers.
/// </summary>
public sealed class HeatmapRenderFrameTests
{
    [Fact]
    public void FromScan_PacksNormalizedValuesAsBase64Bytes()
    {
        var scan = new ParsedFootScan(
            new FootHeatmap(FootSide.Left, 2, 1, [0, 1]),
            new FootHeatmap(FootSide.Right, 2, 1, [.5, .25]));

        var frame = HeatmapRenderFrame.FromScan(scan, DateTimeOffset.UnixEpoch);
        var leftBytes = Convert.FromBase64String(frame.Left.Data);

        Assert.Equal("bicubic", frame.Interpolation);
        Assert.Equal("raw-pressure-base64", frame.Transport);
        Assert.Equal([0, 255], leftBytes);
        Assert.Equal("base64:u8-normalized", frame.Left.Encoding);
    }
}
