namespace FootHeatmapAnalyzer.SensorAlignment.Models;

/// <summary>
/// Connects one pressure sample to the nearest DTW-aligned accelerometer sample.
/// </summary>
public sealed record AlignedSensorSample(int PressureIndex, int AccelerometerIndex, PressureSample Pressure, AccelerometerSample Accelerometer);
