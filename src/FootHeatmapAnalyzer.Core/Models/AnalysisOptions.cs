namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Centralizes threshold values used by feature extraction and rule-based screening.
/// </summary>
public sealed class AnalysisOptions
{
    /// <summary>
    /// Minimum normalized cell value counted as active contact.
    /// </summary>
    public double ContactThreshold { get; init; } = .18;

    /// <summary>
    /// Minimum normalized cell value counted as a local hotspot.
    /// </summary>
    public double HotspotThreshold { get; init; } = .88;

    /// <summary>
    /// Combined arch index below this value is treated as a high-arch tendency.
    /// </summary>
    public double HighArchUpperBound { get; init; } = .20;

    /// <summary>
    /// Combined arch index above this value is treated as a low-arch tendency.
    /// </summary>
    public double LowArchLowerBound { get; init; } = .31;

    /// <summary>
    /// Left load share below this value is treated as right-heavy.
    /// </summary>
    public double LeftLoadLowerBound { get; init; } = .43;

    /// <summary>
    /// Left load share above this value is treated as left-heavy.
    /// </summary>
    public double LeftLoadUpperBound { get; init; } = .57;

    /// <summary>
    /// Minimum forefoot-to-heel asymmetry that triggers a gait review message.
    /// </summary>
    public double ForeHeelAsymmetryThreshold { get; init; } = .18;

    /// <summary>
    /// Minimum average pressure asymmetry that triggers balance review.
    /// </summary>
    public double AveragePressureAsymmetryThreshold { get; init; } = .12;

    /// <summary>
    /// Minimum mirrored pressure-center gap that triggers balance review.
    /// </summary>
    public double CenterGapThreshold { get; init; } = .18;

    /// <summary>
    /// Minimum forefoot/heel load gap that triggers distribution review.
    /// </summary>
    public double ForeHeelGapThreshold { get; init; } = .22;

    /// <summary>
    /// Contact-area threshold below which the scan is considered sparse.
    /// </summary>
    public double SparseContactThreshold { get; init; } = .35;

    /// <summary>
    /// Contact-area threshold used with high arch index for low-arch pattern review.
    /// </summary>
    public double LowArchContactThreshold { get; init; } = .58;

    /// <summary>
    /// Contact-area threshold used with low arch index for high-arch pattern review.
    /// </summary>
    public double HighArchContactThreshold { get; init; } = .48;

    /// <summary>
    /// Upper row ratio used to separate the forefoot region.
    /// </summary>
    public double ForefootRegionEndRatio { get; init; } = .35;

    /// <summary>
    /// Upper row ratio used to separate the midfoot region.
    /// </summary>
    public double MidfootRegionEndRatio { get; init; } = .70;

    /// <summary>
    /// Lower bound used to keep arch-index division stable for near-empty scans.
    /// </summary>
    public double MinimumArchLoadDenominator { get; init; } = .01;

    /// <summary>
    /// Minimum number of hotspot cells that triggers hotspot review.
    /// </summary>
    public int HotspotCountReviewThreshold { get; init; } = 3;
}
