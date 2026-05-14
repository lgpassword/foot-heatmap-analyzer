using FootHeatmapAnalyzer.SensorAlignment.Models;

namespace FootHeatmapAnalyzer.SensorAlignment.Services;

/// <summary>
/// Aligns non-uniform pressure and accelerometer time series.
/// </summary>
public interface ISensorAlignmentService
{
    /// <summary>
    /// Uses Dynamic Time Warping to align pressure load and acceleration magnitude samples.
    /// </summary>
    SensorAlignmentResult Align(IReadOnlyList<PressureSample> pressure, IReadOnlyList<AccelerometerSample> accelerometer);
}
