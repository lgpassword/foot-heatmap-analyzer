namespace FootHeatmapAnalyzer.Web.Models;

/// <summary>
/// Represents one foot as a normalized two-dimensional sensor matrix.
/// </summary>
public sealed record FootHeatmap(FootSide Side, int Width, int Height, double[][] Values)
{
    /// <summary>
    /// Returns all matrix values as a flat sequence for aggregate calculations.
    /// </summary>
    public IEnumerable<double> AllValues() => Values.SelectMany(row => row);
}
