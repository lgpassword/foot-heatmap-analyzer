namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Classifies the left-right load distribution without depending on UI wording.
/// </summary>
public enum BalanceState
{
    /// <summary>
    /// The right foot carries more load than the balanced range.
    /// </summary>
    RightHeavy,

    /// <summary>
    /// The left and right feet are inside the balanced load range.
    /// </summary>
    Balanced,

    /// <summary>
    /// The left foot carries more load than the balanced range.
    /// </summary>
    LeftHeavy
}
