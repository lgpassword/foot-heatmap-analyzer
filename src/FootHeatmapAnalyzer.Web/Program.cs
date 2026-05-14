using FootHeatmapAnalyzer.Composition;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;
using FootHeatmapAnalyzer.SensorAlignment.Models;
using FootHeatmapAnalyzer.SensorAlignment.Services;
using FootHeatmapAnalyzer.Web.Hubs;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
// Keeps framework-level request limits aligned with the page upload limit.
const long maxUploadBytes = 1024 * 1024;

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddFootHeatmapAnalyzer();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; connect-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; script-src 'self'";
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapPost("/api/analyze", async (HttpRequest request, IFootScanParser parser, IFootAnalysisService analysisService) =>
{
    using var memory = new MemoryStream();
    await request.Body.CopyToAsync(memory, request.HttpContext.RequestAborted);

    return AnalyzePayload(
        () => parser.ParseBytes(memory.ToArray()),
        analysisService);
});
app.MapPost("/api/analyze/text", async (HttpRequest request, IFootScanParser parser, IFootAnalysisService analysisService) =>
{
    using var reader = new StreamReader(request.Body);
    var text = await reader.ReadToEndAsync(request.HttpContext.RequestAborted);

    return AnalyzePayload(
        () => parser.ParseText(text),
        analysisService);
});
app.MapPost("/api/render-frame", async (HttpRequest request, IFootScanParser parser) =>
{
    using var memory = new MemoryStream();
    await request.Body.CopyToAsync(memory, request.HttpContext.RequestAborted);

    return ParsePayload(() => HeatmapRenderFrame.FromScan(parser.ParseBytes(memory.ToArray()), DateTimeOffset.UtcNow));
});
app.MapPost("/api/render-frame/text", async (HttpRequest request, IFootScanParser parser) =>
{
    using var reader = new StreamReader(request.Body);
    var text = await reader.ReadToEndAsync(request.HttpContext.RequestAborted);

    return ParsePayload(() => HeatmapRenderFrame.FromScan(parser.ParseText(text), DateTimeOffset.UtcNow));
});
app.MapPost("/api/gait/analyze", (IReadOnlyList<PressureSequenceFrame> sequence, IGaitAnalysisService gaitAnalysisService) =>
{
    try
    {
        return Results.Ok(gaitAnalysisService.Predict(sequence));
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FileNotFoundException)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});
app.MapPost("/api/sensors/align", (SensorAlignmentRequest request, ISensorAlignmentService alignmentService) =>
{
    try
    {
        return Results.Ok(alignmentService.Align(request.Pressure, request.Accelerometer));
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});
app.MapHub<HeatmapStreamHub>("/hubs/heatmap");
app.MapRazorPages();

app.Run();

/// <summary>
/// Converts parsed scan payloads into HTTP API responses.
/// </summary>
static IResult AnalyzePayload(Func<FootHeatmapAnalyzer.Core.Models.ParsedFootScan> parse, IFootAnalysisService analysisService)
{
    try
    {
        return Results.Ok(analysisService.Analyze(parse()));
    }
    catch (Exception ex) when (ex is InvalidDataException or FormatException)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
}

/// <summary>
/// Converts parser-driven payload operations into HTTP API responses.
/// </summary>
static IResult ParsePayload<T>(Func<T> parse)
{
    try
    {
        return Results.Ok(parse());
    }
    catch (Exception ex) when (ex is InvalidDataException or FormatException)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
}

/// <summary>
/// Carries pressure and accelerometer streams for DTW alignment.
/// </summary>
public sealed record SensorAlignmentRequest(IReadOnlyList<PressureSample> Pressure, IReadOnlyList<AccelerometerSample> Accelerometer);

/// <summary>
/// Exposes the top-level web entry point to integration tests.
/// </summary>
public partial class Program;
