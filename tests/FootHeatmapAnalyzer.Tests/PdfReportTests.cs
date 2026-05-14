using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.Web.Dashboard;
using FootHeatmapAnalyzer.Web.Reports;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// 覆盖 QuestPDF 报告生成的字节输出契约。
/// </summary>
public sealed class PdfReportTests
{
    [Fact]
    public void GeneratePdf_ReturnsNonEmptyByteArray()
    {
        var pdf = GeneratePdf();

        Assert.NotEmpty(pdf);
    }

    [Fact]
    public void GeneratePdf_ResultStartsWithPdfMagicBytes()
    {
        var pdf = GeneratePdf();

        Assert.Equal((byte)'%', pdf[0]);
        Assert.Equal((byte)'P', pdf[1]);
        Assert.Equal((byte)'D', pdf[2]);
        Assert.Equal((byte)'F', pdf[3]);
    }

    [Fact]
    public void GeneratePdf_DoesNotThrowOnMinimalInput()
    {
        var exception = Record.Exception(() => GeneratePdf());

        Assert.Null(exception);
    }

    private static byte[] GeneratePdf()
    {
        var parser = new FootScanParser();
        var extractor = new HeatmapFeatureExtractor();
        var scan = parser.ParseBytes(ParsedFootScan.CreateSampleBytes());
        var metrics = extractor.Extract(scan);
        var report = new FootAnalysisService(extractor, new FootRiskClassifier()).Analyze(scan);
        var dashboard = new DashboardService().Build(metrics);

        return new QuestPdfReportGenerator().Generate(report, dashboard, scan, metrics, "binary");
    }
}
