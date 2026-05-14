using FootHeatmapAnalyzer.GaitAnalysis.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FootHeatmapAnalyzer.GaitAnalysis.Services;

/// <summary>
/// Uses ONNX Runtime or the built-in sequence classifier to classify pressure sequences on the server.
/// </summary>
public sealed class OnnxGaitAnalysisService : IGaitAnalysisService, IDisposable
{
    private readonly GaitAnalysisOptions options;
    private InferenceSession? session;

    /// <summary>
    /// Creates an ONNX-backed gait analysis service.
    /// </summary>
    public OnnxGaitAnalysisService(GaitAnalysisOptions options)
    {
        this.options = options;
    }

    /// <inheritdoc />
    public GaitPrediction Predict(IReadOnlyList<PressureSequenceFrame> sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        if (sequence.Count == 0)
        {
            throw new ArgumentException("Pressure sequence must contain at least one frame.", nameof(sequence));
        }

        var featureCount = sequence[0].Features.Length;
        if (featureCount == 0 || sequence.Any(frame => frame.Features.Length != featureCount))
        {
            throw new ArgumentException("All sequence frames must contain the same non-empty feature vector.", nameof(sequence));
        }

        if (string.IsNullOrWhiteSpace(options.ModelPath) || !File.Exists(options.ModelPath))
        {
            return PredictWithBuiltInModel(sequence);
        }

        var tensor = new DenseTensor<float>([1, sequence.Count, featureCount]);
        for (var frameIndex = 0; frameIndex < sequence.Count; frameIndex++)
        {
            for (var featureIndex = 0; featureIndex < featureCount; featureIndex++)
            {
                tensor[0, frameIndex, featureIndex] = sequence[frameIndex].Features[featureIndex];
            }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(options.InputName, tensor)
        };
        using var results = GetSession().Run(inputs);
        var probabilities = results.First(result => result.Name == options.OutputName).AsEnumerable<float>().ToArray();
        if (probabilities.Length == 0)
        {
            throw new InvalidOperationException("ONNX gait model returned no probabilities.");
        }

        var labels = options.Labels.Length >= probabilities.Length
            ? options.Labels
            : Enumerable.Range(0, probabilities.Length).Select(index => $"class_{index}").ToArray();
        var scored = probabilities
            .Select((probability, index) => new KeyValuePair<string, float>(labels[index], probability))
            .ToArray();
        var best = scored.MaxBy(pair => pair.Value);

        return new GaitPrediction(best.Key, best.Value, scored.ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        session?.Dispose();
    }

    private InferenceSession GetSession()
    {
        if (session is not null)
        {
            return session;
        }

        session = new InferenceSession(options.ModelPath);
        return session;
    }

    private GaitPrediction PredictWithBuiltInModel(IReadOnlyList<PressureSequenceFrame> sequence)
    {
        var leftSeries = sequence.Select(frame => frame.Features.ElementAtOrDefault(0)).ToArray();
        var rightSeries = sequence.Select(frame => frame.Features.ElementAtOrDefault(1)).ToArray();
        var forefootSeries = sequence.Select(frame => frame.Features.ElementAtOrDefault(2)).ToArray();
        var heelSeries = sequence.Select(frame => frame.Features.ElementAtOrDefault(3)).ToArray();
        var asymmetry = AverageAbsoluteDifference(leftSeries, rightSeries);
        var forefootBias = Average(forefootSeries) - Average(heelSeries);
        var variability = AverageAbsoluteStepChange(sequence.Select(frame => frame.Features.Sum()).ToArray());

        var normalScore = Clamp01(1 - asymmetry - Math.Abs(forefootBias) - (variability * .25f));
        var asymmetryScore = Clamp01(asymmetry * 1.7f);
        var forefootScore = Clamp01(Math.Max(0, forefootBias) * 1.6f);
        var heelScore = Clamp01(Math.Max(0, -forefootBias) * 1.6f);
        var scores = Normalize([normalScore, asymmetryScore, forefootScore, heelScore]);
        var probabilities = options.Labels
            .Take(scores.Length)
            .Select((label, index) => new KeyValuePair<string, float>(label, scores[index]))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        var best = probabilities.MaxBy(pair => pair.Value);

        return new GaitPrediction(best.Key, best.Value, probabilities);
    }

    private static float Average(float[] values) => values.Length == 0 ? 0 : values.Average();

    private static float AverageAbsoluteDifference(float[] first, float[] second)
    {
        var count = Math.Min(first.Length, second.Length);
        if (count == 0)
        {
            return 0;
        }

        var total = 0f;
        for (var index = 0; index < count; index++)
        {
            total += Math.Abs(first[index] - second[index]);
        }

        return total / count;
    }

    private static float AverageAbsoluteStepChange(float[] values)
    {
        if (values.Length < 2)
        {
            return 0;
        }

        var total = 0f;
        for (var index = 1; index < values.Length; index++)
        {
            total += Math.Abs(values[index] - values[index - 1]);
        }

        return total / (values.Length - 1);
    }

    private static float Clamp01(float value) => Math.Clamp(value, 0, 1);

    private static float[] Normalize(float[] values)
    {
        var total = values.Sum();
        if (total <= 0)
        {
            return [1, 0, 0, 0];
        }

        return values.Select(value => value / total).ToArray();
    }
}
