namespace FootHeatmapAnalyzer.SensorAlignment.Services;

/// <summary>
/// 为非等频传感器序列提供线性重采样工具。
/// </summary>
public static class SensorResamplerHelper
{
    /// <summary>
    /// 将按时间排序的采样点线性插值为指定数量的等间隔数值。
    /// </summary>
    public static double[] ResampleToUniform(IReadOnlyList<(double TimestampMs, double Value)> samples, int targetCount)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (samples.Count == 0)
        {
            throw new ArgumentException("Samples must contain at least one item.", nameof(samples));
        }

        if (targetCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetCount), "Target count must be positive.");
        }

        if (samples.Count == 1)
        {
            return Enumerable.Repeat(samples[0].Value, targetCount).ToArray();
        }

        var ordered = samples.OrderBy(sample => sample.TimestampMs).ToArray();
        var start = ordered[0].TimestampMs;
        var end = ordered[^1].TimestampMs;
        if (Math.Abs(end - start) < double.Epsilon)
        {
            return Enumerable.Repeat(ordered[0].Value, targetCount).ToArray();
        }

        var result = new double[targetCount];
        var sourceIndex = 0;
        for (var index = 0; index < targetCount; index++)
        {
            var ratio = targetCount == 1 ? 0 : index / (double)(targetCount - 1);
            var timestamp = start + ((end - start) * ratio);
            while (sourceIndex < ordered.Length - 2 && ordered[sourceIndex + 1].TimestampMs < timestamp)
            {
                sourceIndex++;
            }

            var left = ordered[sourceIndex];
            var right = ordered[Math.Min(sourceIndex + 1, ordered.Length - 1)];
            var span = Math.Max(right.TimestampMs - left.TimestampMs, double.Epsilon);
            var localRatio = Math.Clamp((timestamp - left.TimestampMs) / span, 0, 1);
            result[index] = left.Value + ((right.Value - left.Value) * localRatio);
        }

        return result;
    }
}
