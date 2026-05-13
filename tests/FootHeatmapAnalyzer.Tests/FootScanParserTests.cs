using FootHeatmapAnalyzer.Web.Models;
using FootHeatmapAnalyzer.Web.Services;
using Microsoft.AspNetCore.Http;

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
    public async Task ParseFileAsync_AcceptsHexFiles()
    {
        var parser = new FootScanParser();
        var hex = Convert.ToHexString(ParsedFootScan.CreateSampleBytes());
        var bytes = System.Text.Encoding.UTF8.GetBytes(hex);
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "scan", "sample.hex");

        var scan = await parser.ParseFileAsync(file);

        Assert.Equal(8, scan.LeftFoot.Width);
        Assert.Equal(12, scan.RightFoot.Height);
    }

    [Fact]
    public async Task ParseFileAsync_AcceptsBinaryFiles()
    {
        var parser = new FootScanParser();
        var bytes = ParsedFootScan.CreateSampleBytes();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "scan", "sample.bin");

        var scan = await parser.ParseFileAsync(file);

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
