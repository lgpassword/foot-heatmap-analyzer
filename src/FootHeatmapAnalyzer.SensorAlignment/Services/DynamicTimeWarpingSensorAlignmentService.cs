using FootHeatmapAnalyzer.SensorAlignment.Models;

namespace FootHeatmapAnalyzer.SensorAlignment.Services;

/// <summary>
/// Implements Dynamic Time Warping for pressure and accelerometer stream alignment.
/// </summary>
public sealed class DynamicTimeWarpingSensorAlignmentService : ISensorAlignmentService
{
    /// <inheritdoc />
    public SensorAlignmentResult Align(IReadOnlyList<PressureSample> pressure, IReadOnlyList<AccelerometerSample> accelerometer)
    {
        ArgumentNullException.ThrowIfNull(pressure);
        ArgumentNullException.ThrowIfNull(accelerometer);
        if (pressure.Count == 0 || accelerometer.Count == 0)
        {
            throw new ArgumentException("Both sensor streams must contain at least one sample.");
        }

        var pressureValues = pressure.Select(sample => sample.Load).ToArray();
        var accelerometerValues = accelerometer.Select(sample => sample.Magnitude).ToArray();
        var costs = new double[pressureValues.Length + 1, accelerometerValues.Length + 1];
        for (var i = 0; i <= pressureValues.Length; i++)
        {
            for (var j = 0; j <= accelerometerValues.Length; j++)
            {
                costs[i, j] = double.PositiveInfinity;
            }
        }

        costs[0, 0] = 0;
        for (var i = 1; i <= pressureValues.Length; i++)
        {
            for (var j = 1; j <= accelerometerValues.Length; j++)
            {
                var distance = Math.Abs(pressureValues[i - 1] - accelerometerValues[j - 1]);
                costs[i, j] = distance + Math.Min(costs[i - 1, j], Math.Min(costs[i, j - 1], costs[i - 1, j - 1]));
            }
        }

        return new SensorAlignmentResult(costs[pressure.Count, accelerometer.Count], TracePath(costs, pressure, accelerometer));
    }

    private static IReadOnlyList<AlignedSensorSample> TracePath(
        double[,] costs,
        IReadOnlyList<PressureSample> pressure,
        IReadOnlyList<AccelerometerSample> accelerometer)
    {
        var path = new List<AlignedSensorSample>();
        var i = pressure.Count;
        var j = accelerometer.Count;
        while (i > 0 && j > 0)
        {
            path.Add(new AlignedSensorSample(i - 1, j - 1, pressure[i - 1], accelerometer[j - 1]));

            var diagonal = costs[i - 1, j - 1];
            var up = costs[i - 1, j];
            var left = costs[i, j - 1];
            if (diagonal <= up && diagonal <= left)
            {
                i--;
                j--;
            }
            else if (up <= left)
            {
                i--;
            }
            else
            {
                j--;
            }
        }

        path.Reverse();
        return path;
    }
}
