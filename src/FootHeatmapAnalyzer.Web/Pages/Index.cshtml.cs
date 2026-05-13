using System.Text.Json;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FootHeatmapAnalyzer.Web.Pages;

public sealed class IndexModel(IFootScanParser parser, IFootAnalysisService analysisService) : PageModel
{
    private const long MaxUploadBytes = 1024 * 1024;

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
                LoadScan(await ParseUploadedFileAsync(ScanFile, HttpContext.RequestAborted));
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

    private async Task<ParsedFootScan> ParseUploadedFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length > MaxUploadBytes)
        {
            throw new InvalidDataException($"上传文件不能超过 {MaxUploadBytes / 1024} KB。");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return extension switch
        {
            ".bin" or ".dat" => parser.ParseBytes(bytes),
            ".hex" or ".txt" or ".b64" or ".base64" => parser.ParseText(System.Text.Encoding.UTF8.GetString(bytes)),
            _ => ParseUnknownFile(bytes)
        };
    }

    private ParsedFootScan ParseUnknownFile(byte[] bytes)
    {
        try
        {
            return parser.ParseBytes(bytes);
        }
        catch (InvalidDataException)
        {
            return parser.ParseText(System.Text.Encoding.UTF8.GetString(bytes));
        }
    }
}
