namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Describes the structured left-right load balance state and display text.
/// </summary>
public sealed record BalanceResult(BalanceState State, double LeftShare, string Label)
{
    /// <summary>
    /// Formats the balance state for report display.
    /// </summary>
    public override string ToString() => Label;
}
