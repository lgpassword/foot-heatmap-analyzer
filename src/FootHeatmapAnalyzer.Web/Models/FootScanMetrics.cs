namespace FootHeatmapAnalyzer.Web.Models;

/// <summary>
/// Combines left and right foot features used by the screening classifier.
/// </summary>
public sealed record FootScanMetrics(FootRegionMetrics Left, FootRegionMetrics Right)
{
    /// <summary>
    /// Average arch index across both feet.
    /// </summary>
    public double CombinedArchIndex => (Left.ArchIndex + Right.ArchIndex) / 2;

    /// <summary>
    /// Share of total load measured on the left foot.
    /// </summary>
    public double LeftLoadShare => Left.TotalLoad / Math.Max(Left.TotalLoad + Right.TotalLoad, .01);

    /// <summary>
    /// Difference between left and right average pressure.
    /// </summary>
    public double AveragePressureAsymmetry => Math.Abs(Left.AveragePressure - Right.AveragePressure);

    /// <summary>
    /// Difference between left and right propulsion tendency.
    /// </summary>
    public double ForeHeelAsymmetry => Math.Abs((Left.ForefootLoad - Left.HeelLoad) - (Right.ForefootLoad - Right.HeelLoad));

    /// <summary>
    /// Highest normalized pressure point observed in the scan.
    /// </summary>
    public double PeakPressure => Math.Max(Left.PeakPressure, Right.PeakPressure);

    /// <summary>
    /// Mean ratio of active cells across both feet.
    /// </summary>
    public double ContactAreaRatio => (Left.ContactAreaRatio + Right.ContactAreaRatio) / 2;

    /// <summary>
    /// Count of extreme local pressure points across both feet.
    /// </summary>
    public int HotspotCount => Left.HotspotCount + Right.HotspotCount;
}
