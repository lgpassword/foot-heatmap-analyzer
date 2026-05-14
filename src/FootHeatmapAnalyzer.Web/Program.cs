using FootHeatmapAnalyzer.Composition;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;
using FootHeatmapAnalyzer.SensorAlignment.Models;
using FootHeatmapAnalyzer.SensorAlignment.Services;
using FootHeatmapAnalyzer.Web.Api;
using FootHeatmapAnalyzer.Web.Dashboard;
using FootHeatmapAnalyzer.Web.Hubs;
using FootHeatmapAnalyzer.Web.Identity;
using FootHeatmapAnalyzer.Web.Reports;
using FootHeatmapAnalyzer.Web.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// Keeps framework-level request limits aligned with the page upload limit.
const long maxUploadBytes = 1024 * 1024;

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddFootHeatmapAnalyzer(builder.Configuration);
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options => options.UseInMemoryDatabase("FootHeatmapIdentity"));
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddSignInManager();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IProfileStore, InMemoryProfileStore>();
builder.Services.AddScoped<ITenantContextAccessor, HttpTenantContextAccessor>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IReportGenerator, QuestPdfReportGenerator>();
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
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; connect-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; script-src 'self' https://cdn.jsdelivr.net";
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
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
app.MapPost("/api/gait/analyze", async (HttpRequest request, IFootScanParser parser, IGaitAnalysisService gaitAnalysisService) =>
{
    try
    {
        var sequence = await ReadGaitSequenceAsync(request, parser);
        return Results.Ok(gaitAnalysisService.Predict(sequence));
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FileNotFoundException or InvalidDataException or JsonException)
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
app.MapGet("/api/profiles", (ITenantContextAccessor tenantAccessor, IProfileStore profileStore) =>
{
    return Results.Ok(profileStore.List(tenantAccessor.GetCurrent()));
});
app.MapPost("/api/profiles", (CreateProfileRequest request, ITenantContextAccessor tenantAccessor, IProfileStore profileStore) =>
{
    try
    {
        return Results.Created("/api/profiles", profileStore.Create(tenantAccessor.GetCurrent(), request));
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});
app.MapPost("/api/hardware/scans", (HardwareScanRequest request, IFootScanParser parser, IFootAnalysisService analysisService, IHeatmapFeatureExtractor featureExtractor, IDashboardService dashboardService, ITenantContextAccessor tenantAccessor) =>
{
    return ParsePayload(() =>
    {
        var tenant = string.IsNullOrWhiteSpace(request.TenantId) ? tenantAccessor.GetCurrent().TenantId : request.TenantId;
        var scan = request.PayloadEncoding.Equals("base64", StringComparison.OrdinalIgnoreCase)
            ? parser.ParseBytes(Convert.FromBase64String(request.Payload))
            : parser.ParseText(request.Payload);
        var report = analysisService.Analyze(scan);
        var metrics = featureExtractor.Extract(scan);

        return new HardwareScanResponse(
            Convert.ToHexString(Guid.NewGuid().ToByteArray()),
            request.DeviceId,
            tenant,
            request.ProfileId,
            report,
            HeatmapRenderFrame.FromScan(scan, request.CapturedAt),
            dashboardService.Build(metrics));
    });
});
app.MapPost("/api/dashboard", async (HttpRequest request, IFootScanParser parser, IHeatmapFeatureExtractor featureExtractor, IDashboardService dashboardService) =>
{
    var payload = await ReadBodyAsync(request);
    return ParsePayload(() => dashboardService.Build(featureExtractor.Extract(ParseScanPayload(parser, payload))));
});
app.MapPost("/api/reports/pdf", async (HttpRequest request, IFootScanParser parser, IFootAnalysisService analysisService, IHeatmapFeatureExtractor featureExtractor, IDashboardService dashboardService, IReportGenerator reportGenerator) =>
{
    var payload = await ReadBodyAsync(request);

    try
    {
        var scan = ParseScanPayload(parser, payload);
        var report = analysisService.Analyze(scan);
        var metrics = featureExtractor.Extract(scan);
        var dashboard = dashboardService.Build(metrics);
        return Results.File(reportGenerator.Generate(report, dashboard, scan, metrics, payload.InputFormat), "application/pdf", "foot-pressure-report.pdf");
    }
    catch (Exception ex) when (ex is InvalidDataException or FormatException)
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
/// 读取请求体并保留内容类型，便于同一端点兼容二进制和文本扫描数据。
/// </summary>
static async Task<ScanRequestPayload> ReadBodyAsync(HttpRequest request)
{
    using var memory = new MemoryStream();
    await request.Body.CopyToAsync(memory, request.HttpContext.RequestAborted);
    var bytes = memory.ToArray();
    var contentType = request.ContentType ?? string.Empty;
    var isText = contentType.Contains("text", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);

    return new ScanRequestPayload(bytes, isText ? "text" : "binary", isText);
}

/// <summary>
/// 根据请求内容类型选择文本或二进制解析方式。
/// </summary>
static FootHeatmapAnalyzer.Core.Models.ParsedFootScan ParseScanPayload(IFootScanParser parser, ScanRequestPayload payload)
{
    return payload.IsText
        ? parser.ParseText(System.Text.Encoding.UTF8.GetString(payload.Bytes))
        : parser.ParseBytes(payload.Bytes);
}

/// <summary>
/// 解析步态端点输入，兼容 JSON 压力序列和普通扫描文件。
/// </summary>
static async Task<IReadOnlyList<PressureSequenceFrame>> ReadGaitSequenceAsync(HttpRequest request, IFootScanParser parser)
{
    var payload = await ReadBodyAsync(request);
    var contentType = request.ContentType ?? string.Empty;
    if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
    {
        return JsonSerializer.Deserialize<IReadOnlyList<PressureSequenceFrame>>(
            payload.Bytes,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Pressure sequence JSON is empty.");
    }

    var scan = ParseScanPayload(parser, payload);
    return BuildSequenceFromScan(scan);
}

/// <summary>
/// 将静态扫描矩阵转换为演示用步态特征序列。
/// </summary>
static IReadOnlyList<PressureSequenceFrame> BuildSequenceFromScan(FootHeatmapAnalyzer.Core.Models.ParsedFootScan scan)
{
    var rows = Math.Min(scan.LeftFoot.Height, scan.RightFoot.Height);
    var sequence = new List<PressureSequenceFrame>(rows);
    for (var row = 0; row < rows; row++)
    {
        sequence.Add(new PressureSequenceFrame(
            TimeSpan.FromMilliseconds(row * 20),
            [
                RowAverage(scan.LeftFoot, row),
                RowAverage(scan.RightFoot, row),
                (float)scan.LeftFoot.At(scan.LeftFoot.Width / 2, row),
                (float)scan.RightFoot.At(scan.RightFoot.Width / 2, row)
            ]));
    }

    return sequence;
}

/// <summary>
/// 计算单行归一化压力均值，用作步态占位输入特征。
/// </summary>
static float RowAverage(FootHeatmapAnalyzer.Core.Models.FootHeatmap heatmap, int row)
{
    double total = 0;
    for (var column = 0; column < heatmap.Width; column++)
    {
        total += heatmap.At(column, row);
    }

    return (float)(total / heatmap.Width);
}

/// <summary>
/// Carries pressure and accelerometer streams for DTW alignment.
/// </summary>
public sealed record SensorAlignmentRequest(IReadOnlyList<PressureSample> Pressure, IReadOnlyList<AccelerometerSample> Accelerometer);

/// <summary>
/// 保存原始请求体及其扫描输入格式。
/// </summary>
public sealed record ScanRequestPayload(byte[] Bytes, string InputFormat, bool IsText);

/// <summary>
/// Exposes the top-level web entry point to integration tests.
/// </summary>
public partial class Program;
