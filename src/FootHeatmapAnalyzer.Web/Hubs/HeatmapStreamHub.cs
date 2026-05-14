using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using Microsoft.AspNetCore.SignalR;

namespace FootHeatmapAnalyzer.Web.Hubs;

/// <summary>
/// Streams compact raw pressure frames to browser clients for local GPU or Canvas rendering.
/// </summary>
public sealed class HeatmapStreamHub(IFootScanParser parser) : Hub
{
    /// <summary>
    /// Parses a text payload and returns a compressed render frame to the caller.
    /// </summary>
    public HeatmapRenderFrame BuildRenderFrameFromText(string payload)
    {
        return HeatmapRenderFrame.FromScan(parser.ParseText(payload), DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Sends a deterministic sample render frame to the caller for live rendering bootstrap.
    /// </summary>
    public async Task SendSampleFrameAsync()
    {
        var frame = HeatmapRenderFrame.FromScan(parser.ParseBytes(ParsedFootScan.CreateSampleBytes()), DateTimeOffset.UtcNow);
        await Clients.Caller.SendAsync("heatmapFrame", frame);
    }
}
