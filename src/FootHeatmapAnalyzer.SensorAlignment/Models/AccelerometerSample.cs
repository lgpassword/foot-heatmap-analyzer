namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// Represents one timestamped phone accelerometer sample.
/// </summary>
public sealed record AccelerometerSample(TimeSpan Timestamp, double X, double Y, double Z)
{
    /// <summary>
    /// Combined acceleration magnitude used by DTW distance calculations.
    /// </summary>
    public double Magnitude => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
}
