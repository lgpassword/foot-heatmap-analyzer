using FootHeatmapAnalyzer.GaitAnalysis.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FootHeatmapAnalyzer.GaitAnalysis.Services;

/// <summary>
/// 使用 ONNX Runtime 在服务端识别压力序列步态类别。
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
            return new GaitPrediction(
                "ModelNotConfigured",
                0,
                new Dictionary<string, float>(),
                true,
                "No ONNX model path is configured. Set GaitAnalysis:ModelPath in appsettings.json to enable predictions.");
        }

        var tensor = new DenseTensor<float>([1, sequence.Count * featureCount]);
        for (var frameIndex = 0; frameIndex < sequence.Count; frameIndex++)
        {
            for (var featureIndex = 0; featureIndex < featureCount; featureIndex++)
            {
                tensor[0, (frameIndex * featureCount) + featureIndex] = sequence[frameIndex].Features[featureIndex];
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

}
