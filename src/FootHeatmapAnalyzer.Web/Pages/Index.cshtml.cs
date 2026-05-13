using System.Text.Json;
using FootHeatmapAnalyzer.Web.Models;
using FootHeatmapAnalyzer.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FootHeatmapAnalyzer.Web.Pages;

public sealed class IndexModel(IFootScanParser parser, IFootAnalysisService analysisService) : PageModel
{
    [BindProperty]
    public string? ScanText { get; set; }

    [BindProperty]
    public IFormFile? ScanFile { get; set; }

    public string SampleHex { get; private set; } = Convert.ToHexString(ParsedFootScan.CreateSampleBytes());

    public ParsedFootScan? Scan { get; private set; }

    public FootAnalysisReport? Report { get; private set; }

    public string? HeatmapJson { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        LoadScan(parser.ParseBytes(ParsedFootScan.CreateSampleBytes()));
    }

    public async Task OnPostAsync()
    {
        try
        {
            if (ScanFile is { Length: > 0 })
            {
                LoadScan(await parser.ParseFileAsync(ScanFile, HttpContext.RequestAborted));
                return;
            }

            LoadScan(parser.ParseText(ScanText ?? string.Empty));
        }
        catch (Exception ex) when (ex is InvalidDataException or FormatException)
        {
            ErrorMessage = ex.Message;
            LoadScan(parser.ParseBytes(ParsedFootScan.CreateSampleBytes()));
        }
    }

    private void LoadScan(ParsedFootScan scan)
    {
        Scan = scan;
        Report = analysisService.Analyze(scan);
        HeatmapJson = JsonSerializer.Serialize(new
        {
            left = scan.LeftFoot.Values,
            right = scan.RightFoot.Values
        });
    }
}
