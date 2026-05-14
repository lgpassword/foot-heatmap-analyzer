using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Web.Dashboard;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FootHeatmapAnalyzer.Web.Reports;

/// <summary>
/// Generates PDF reports with QuestPDF for preview and printing.
/// </summary>
public sealed class QuestPdfReportGenerator : IReportGenerator
{
    /// <inheritdoc />
    public byte[] Generate(FootAnalysisReport report, DashboardPayload dashboard)
    {
        var fallbackScan = ParsedFootScan.CreateSampleBytes();
        var parser = new FootHeatmapAnalyzer.Core.Services.FootScanParser();
        var scan = parser.ParseBytes(fallbackScan);
        var metrics = new FootHeatmapAnalyzer.Algorithms.Services.HeatmapFeatureExtractor().Extract(scan);
        return Generate(report, dashboard, scan, metrics, "sample");
    }

    /// <inheritdoc />
    public byte[] Generate(FootAnalysisReport report, DashboardPayload dashboard, ParsedFootScan scan, FootScanMetrics metrics, string inputFormat)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(dashboard);
        ArgumentNullException.ThrowIfNull(scan);
        ArgumentNullException.ThrowIfNull(metrics);
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(style => style.FontSize(10));
                page.Header().Column(header =>
                {
                    header.Item().Text("Foot Pressure Analysis Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);
                    header.Item().Text("Non-diagnostic screening output").FontSize(11).FontColor(Colors.Grey.Darken2);
                    header.Item().Text($"Scan date/time: {DateTimeOffset.UtcNow:u}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Border(1).BorderColor(Colors.Red.Medium).Padding(8).Text("This report is for research and educational use only. It does not constitute a medical diagnosis.").FontColor(Colors.Red.Darken2);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });
                        AddMetricRow(table, "Arch Index (Left / Right)", $"{metrics.Left.ArchIndex:F3} / {metrics.Right.ArchIndex:F3}");
                        AddMetricRow(table, "Contact Area (Left / Right)", $"{metrics.Left.ContactAreaRatio:P1} / {metrics.Right.ContactAreaRatio:P1}");
                        AddMetricRow(table, "Forefoot Load % (Left / Right)", $"{metrics.Left.ForefootLoad:P1} / {metrics.Right.ForefootLoad:P1}");
                        AddMetricRow(table, "Heel Load % (Left / Right)", $"{metrics.Left.HeelLoad:P1} / {metrics.Right.HeelLoad:P1}");
                        AddMetricRow(table, "Center-of-Pressure X offset", $"{metrics.Left.CenterX - .5:F3} / {metrics.Right.CenterX - .5:F3}");
                        AddMetricRow(table, "Center-of-Pressure Y offset", $"{metrics.Left.CenterY - .5:F3} / {metrics.Right.CenterY - .5:F3}");
                        AddMetricRow(table, "Left/Right Load Balance %", $"{dashboard.LoadBalance.LeftPercent:F1}% / {dashboard.LoadBalance.RightPercent:F1}%");
                        AddMetricRow(table, "Peak Pressure (Left / Right)", $"{metrics.Left.PeakPressure:F3} / {metrics.Right.PeakPressure:F3}");
                        AddMetricRow(table, "Hotspot Count (Left / Right)", $"{metrics.Left.HotspotCount} / {metrics.Right.HotspotCount}");
                    });
                    column.Item().Text("Screening Classification").SemiBold().FontSize(14);
                    foreach (var finding in report.Findings)
                    {
                        column.Item().Text($"{finding.Category} - {finding.Level}").SemiBold().FontColor(ColorForLevel(finding.Level));
                        column.Item().Text(finding.Summary);
                    }
                });
                AddFooter(page);
            });

            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(style => style.FontSize(10));
                page.Header().Text("Raw Data Summary").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Width x Height: {scan.LeftFoot.Width} x {scan.LeftFoot.Height}");
                    column.Item().Text($"Input format: {inputFormat}");
                    column.Item().Text($"Left min / max / mean: {scan.LeftFoot.Values.Min():F3} / {scan.LeftFoot.Values.Max():F3} / {scan.LeftFoot.Values.Average():F3}");
                    column.Item().Text($"Right min / max / mean: {scan.RightFoot.Values.Min():F3} / {scan.RightFoot.Values.Max():F3} / {scan.RightFoot.Values.Average():F3}");
                    column.Item().LineHorizontal(1);
                    column.Item().Text(report.Disclaimer).FontColor(Colors.Red.Darken2);
                });
                AddFooter(page);
            });
        }).GeneratePdf();
    }

    private static void AddMetricRow(TableDescriptor table, string label, string value)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(label).SemiBold();
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(value);
    }

    private static string ColorForLevel(string level)
    {
        return level switch
        {
            "Normal" or "较低" or "可用" => Colors.Green.Darken2,
            "Mild" or "观察" or "持续跟踪" => Colors.Yellow.Darken3,
            "Moderate" or "需复核" => Colors.Orange.Darken3,
            "Severe" or "谨慎" => Colors.Red.Darken2,
            _ => Colors.Grey.Darken3
        };
    }

    private static void AddFooter(PageDescriptor page)
    {
        page.Footer().Row(row =>
        {
            row.RelativeItem().Text("Generated by foot-heatmap-analyzer (MIT License) — Not a medical device").FontSize(8).FontColor(Colors.Grey.Darken2);
            row.ConstantItem(80).AlignRight().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }
}
