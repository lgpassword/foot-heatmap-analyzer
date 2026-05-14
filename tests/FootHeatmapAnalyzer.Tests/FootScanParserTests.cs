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
        Assert.Equal(96, scan.RightFoot.Values.Count);
        Assert.InRange(scan.LeftFoot.At(0, 0), 0, 1);
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
    public void ParseText_AcceptsBinaryPayload()
    {
        var parser = new FootScanParser();
        var binary = string.Concat(ParsedFootScan.CreateSampleBytes().Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));

        var scan = parser.ParseText(binary);

        Assert.Equal(FootSide.Left, scan.LeftFoot.Side);
        Assert.Equal(FootSide.Right, scan.RightFoot.Side);
    }

    [Fact]
    public void ParseText_AcceptsBase64Payload()
    {
        var parser = new FootScanParser();
        var base64 = Convert.ToBase64String(ParsedFootScan.CreateSampleBytes());

        var scan = parser.ParseText(base64);

        Assert.Equal(FootSide.Left, scan.LeftFoot.Side);
        Assert.Equal(FootSide.Right, scan.RightFoot.Side);
    }

    [Fact]
    public void ParseText_RejectsUnknownPayloadWithFriendlyMessage()
    {
        var parser = new FootScanParser();

        var error = Assert.Throws<InvalidDataException>(() => parser.ParseText("not valid scan payload!"));

        Assert.Contains("无法识别", error.Message);
    }

    [Fact]
    public void ParseBytes_RejectsInvalidLength()
    {
        var parser = new FootScanParser();

        Assert.Throws<InvalidDataException>(() => parser.ParseBytes([2, 2, 1, 2]));
    }

    [Fact]
    public void FootHeatmap_UsesReferenceEqualityForDistinctBuffers()
    {
        var first = new FootHeatmap(FootSide.Left, 1, 1, [1d]);
        var second = new FootHeatmap(FootSide.Left, 1, 1, [1d]);

        Assert.NotEqual(first, second);
        Assert.Equal(1d, first.At(0, 0));
    }
}
