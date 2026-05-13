using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

namespace FootHeatmapAnalyzer.Tests;

public sealed class FootScanParserTests
{
    [Fact]
    public void ParseBytes_ReturnsLeftAndRightMatrices()
    {
        var parser = new FootScanParser();

        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());

        Assert.Equal(8, scan.LeftFoot.Width);
        Assert.Equal(12, scan.LeftFoot.Height);
        Assert.Equal(8, scan.RightFoot.Values[0].Length);
        Assert.InRange(scan.LeftFoot.Values[0][0], 0, 1);
    }

    [Fact]
    public void ParseText_AcceptsHexPayload()
    {
        var parser = new FootScanParser();
        var hex = Convert.ToHexString(ParsedFootScan.CreateSampleBytes());

        var scan = parser.ParseText(hex);

        Assert.Equal(FootSide.Left, scan.LeftFoot.Side);
        Assert.Equal(FootSide.Right, scan.RightFoot.Side);
    }

    [Fact]
    public void ParseBytes_RejectsInvalidLength()
    {
        var parser = new FootScanParser();

        Assert.Throws<InvalidDataException>(() => parser.ParseBytes([2, 2, 1, 2]));
    }
}
