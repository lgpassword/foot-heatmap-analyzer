using FootHeatmapAnalyzer.SensorAlignment.Models;
using FootHeatmapAnalyzer.SensorAlignment.Services;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// 覆盖 DTW 传感器对齐算法的核心路径和边界条件。
/// </summary>
public sealed class DtwAlignmentTests
{
    [Fact]
    public void Align_EqualLengthSeries_ReturnsZeroDistance()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        var result = service.Align(
            [new PressureSample(TimeSpan.Zero, 1), new PressureSample(TimeSpan.FromMilliseconds(10), 2)],
            [new AccelerometerSample(TimeSpan.Zero, 1, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(10), 2, 0, 0)]);

        Assert.Equal(0, result.NormalizedDistance);
    }

    [Fact]
    public void Align_IdenticalSeries_ReturnsZeroDistance()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        var result = service.Align(
            [new PressureSample(TimeSpan.Zero, .5), new PressureSample(TimeSpan.FromMilliseconds(10), .75), new PressureSample(TimeSpan.FromMilliseconds(20), 1)],
            [new AccelerometerSample(TimeSpan.Zero, .5, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(10), .75, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(20), 1, 0, 0)]);

        Assert.Equal(0, result.Cost);
        Assert.Equal(0, result.NormalizedDistance);
    }

    [Fact]
    public void Align_ShiftedSeries_ReturnsCorrectPath()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        var result = service.Align(
            [new PressureSample(TimeSpan.Zero, 1), new PressureSample(TimeSpan.FromMilliseconds(10), 2), new PressureSample(TimeSpan.FromMilliseconds(20), 3)],
            [new AccelerometerSample(TimeSpan.Zero, 1, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(5), 1, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(15), 2, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(25), 3, 0, 0)]);

        Assert.Equal(new AlignmentPathPoint(0, 0), result.AlignmentPath[0]);
        Assert.Equal(new AlignmentPathPoint(2, 3), result.AlignmentPath[^1]);
        Assert.True(result.AlignmentPath.Count >= 3);
    }

    [Fact]
    public void Align_EmptyPressureSeries_ThrowsArgumentException()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        Assert.Throws<ArgumentException>(() => service.Align([], [new AccelerometerSample(TimeSpan.Zero, 1, 0, 0)]));
    }

    [Fact]
    public void Align_SingleElementSeries_ReturnsPath()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        var result = service.Align(
            [new PressureSample(TimeSpan.Zero, 1)],
            [new AccelerometerSample(TimeSpan.Zero, 1, 0, 0)]);

        Assert.Single(result.AlignmentPath);
        Assert.Equal(0, result.AlignmentPath[0].PressureIndex);
        Assert.Equal(0, result.AlignmentPath[0].AccelerometerIndex);
    }

    [Fact]
    public void NormalizedDistance_IsAlwaysNonNegative()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        var result = service.Align(
            [new PressureSample(TimeSpan.Zero, 1), new PressureSample(TimeSpan.FromMilliseconds(10), 4)],
            [new AccelerometerSample(TimeSpan.Zero, 2, 0, 0), new AccelerometerSample(TimeSpan.FromMilliseconds(10), 8, 0, 0)]);

        Assert.True(result.NormalizedDistance >= 0);
    }
}
