using FootHeatmapAnalyzer.SensorAlignment.Models;
using FootHeatmapAnalyzer.SensorAlignment.Services;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers Dynamic Time Warping alignment for pressure and phone accelerometer streams.
/// </summary>
public sealed class SensorAlignmentTests
{
    [Fact]
    public void Align_ReturnsMonotonicDtwPath()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();
        var pressure = new[]
        {
            new PressureSample(TimeSpan.Zero, 1),
            new PressureSample(TimeSpan.FromMilliseconds(20), 2),
            new PressureSample(TimeSpan.FromMilliseconds(40), 3)
        };
        var accelerometer = new[]
        {
            new AccelerometerSample(TimeSpan.Zero, 1, 0, 0),
            new AccelerometerSample(TimeSpan.FromMilliseconds(10), 2, 0, 0),
            new AccelerometerSample(TimeSpan.FromMilliseconds(30), 2.9, 0, 0),
            new AccelerometerSample(TimeSpan.FromMilliseconds(50), 3, 0, 0)
        };

        var result = service.Align(pressure, accelerometer);

        Assert.True(result.Cost >= 0);
        Assert.NotEmpty(result.Path);
        Assert.Equal(0, result.Path[0].PressureIndex);
        Assert.Equal(pressure.Length - 1, result.Path[^1].PressureIndex);
        Assert.All(result.Path.Zip(result.Path.Skip(1)), pair =>
        {
            Assert.True(pair.First.PressureIndex <= pair.Second.PressureIndex);
            Assert.True(pair.First.AccelerometerIndex <= pair.Second.AccelerometerIndex);
        });
    }

    [Fact]
    public void Align_ProducesExpectedDiagonalPathForEqualSeries()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();
        var pressure = new[]
        {
            new PressureSample(TimeSpan.Zero, 1),
            new PressureSample(TimeSpan.FromMilliseconds(10), 2)
        };
        var accelerometer = new[]
        {
            new AccelerometerSample(TimeSpan.Zero, 1, 0, 0),
            new AccelerometerSample(TimeSpan.FromMilliseconds(10), 2, 0, 0)
        };

        var result = service.Align(pressure, accelerometer);

        Assert.Equal(2, result.Path.Count);
        Assert.Equal(0, result.Path[0].PressureIndex);
        Assert.Equal(0, result.Path[0].AccelerometerIndex);
        Assert.Equal(1, result.Path[1].PressureIndex);
        Assert.Equal(1, result.Path[1].AccelerometerIndex);
        Assert.True(result.Cost < .001);
    }

    [Fact]
    public void Align_RejectsEmptyStreams()
    {
        var service = new DynamicTimeWarpingSensorAlignmentService();

        Assert.Throws<ArgumentException>(() => service.Align([], []));
    }
}
