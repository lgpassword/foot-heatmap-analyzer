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
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(dashboard);
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(style => style.FontSize(10));
                page.Header().Text("Foot Pressure Analysis Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text($"Arch: {report.ArchType}");
                    column.Item().Text($"Gait: {report.GaitCycle}");
                    column.Item().Text($"Center of pressure: {report.CenterOfPressure}");
                    column.Item().Text($"Load balance: L {dashboard.LoadBalance.LeftPercent}% / R {dashboard.LoadBalance.RightPercent}%");
                    column.Item().LineHorizontal(1);
                    foreach (var finding in report.Findings)
                    {
                        column.Item().Text($"{finding.Category} - {finding.Level}").SemiBold();
                        column.Item().Text(finding.Summary);
                        column.Item().Text(finding.Rationale).FontColor(Colors.Grey.Darken2);
                    }

                    column.Item().LineHorizontal(1);
                    column.Item().Text(report.Disclaimer).FontColor(Colors.Red.Darken2);
                });
                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Generated ");
                    text.Span(DateTimeOffset.UtcNow.ToString("u"));
                });
            });
        }).GeneratePdf();
    }
}
