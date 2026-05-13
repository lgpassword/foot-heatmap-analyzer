namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Captures measurable heatmap features for one foot.
/// </summary>
public sealed record FootRegionMetrics(
    FootSide Side,
    double ForefootLoad,
    double MidfootLoad,
    double HeelLoad,
    double TotalLoad,
    double AveragePressure,
    double PeakPressure,
    double ContactAreaRatio,
    double CenterX,
    double CenterY,
    int HotspotCount,
    double ArchIndex);
