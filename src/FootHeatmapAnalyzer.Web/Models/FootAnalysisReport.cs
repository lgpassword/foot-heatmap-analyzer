namespace FootHeatmapAnalyzer.Web.Models;

/// <summary>
/// Groups all auxiliary screening outputs shown to the user.
/// </summary>
public sealed record FootAnalysisReport(
    string ArchType,
    string GaitCycle,
    string CenterOfPressure,
    IReadOnlyList<AnalysisFinding> Findings,
    string Disclaimer);
