using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootHeatmapAnalyzer.Composition;

/// <summary>
/// Centralizes application service registration across core and algorithm layers.
/// </summary>
public static class FootHeatmapServiceCollectionExtensions
{
    public static IServiceCollection AddFootHeatmapAnalyzer(this IServiceCollection services)
    {
        services.AddSingleton<AnalysisOptions>();
        services.AddSingleton<IFootScanParser, FootScanParser>();
        services.AddSingleton<IHeatmapFeatureExtractor, HeatmapFeatureExtractor>();
        services.AddSingleton<IFootRiskClassifier, FootRiskClassifier>();
        services.AddSingleton<IFootAnalysisService, FootAnalysisService>();

        return services;
    }
}
