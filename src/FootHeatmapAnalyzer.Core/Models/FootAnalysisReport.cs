namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Groups all auxiliary screening outputs shown to the user.
/// </summary>
public sealed record FootAnalysisReport(
    string ArchType,
    string GaitCycle,
    string CenterOfPressure,
    IReadOnlyList<AnalysisFinding> Findings,
    string Disclaimer)
{
    /// <summary>
    /// 左右足前足/足跟负载比例，已限制在 0.1 到 10.0。
    /// </summary>
    public SidePair<double> ForefootHeelRatio { get; init; } = new(0, 0);

    /// <summary>
    /// 左右足足弓指数类别，High/Normal/Flat。
    /// </summary>
    public SidePair<string> ArchIndexCategory { get; init; } = new("Normal", "Normal");

    /// <summary>
    /// 左右总负载对称性评分，范围 0 到 1。
    /// </summary>
    public double LoadSymmetryScore { get; init; }

    /// <summary>
    /// 左右足热点数量对应的严重程度。
    /// </summary>
    public SidePair<string> HotspotSeverity { get; init; } = new("None", "None");
}
