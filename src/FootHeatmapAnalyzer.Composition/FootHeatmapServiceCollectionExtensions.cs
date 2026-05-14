using FootHeatmapAnalyzer.Algorithms.Services;
using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;
using FootHeatmapAnalyzer.GaitAnalysis.Models;
using FootHeatmapAnalyzer.GaitAnalysis.Services;
using FootHeatmapAnalyzer.SensorAlignment.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FootHeatmapAnalyzer.Composition;

/// <summary>
/// Centralizes application service registration across core and algorithm layers.
/// </summary>
public static class FootHeatmapServiceCollectionExtensions
{
    /// <summary>
    /// 注册默认足底压力分析服务，不绑定配置文件。
    /// </summary>
    public static IServiceCollection AddFootHeatmapAnalyzer(this IServiceCollection services)
    {
        return services.AddFootHeatmapAnalyzer(null);
    }

    /// <summary>
    /// 注册足底压力分析服务，并从配置文件绑定算法选项。
    /// </summary>
    public static IServiceCollection AddFootHeatmapAnalyzer(this IServiceCollection services, IConfiguration? configuration)
    {
        services.AddSingleton<AnalysisOptions>();
        if (configuration is null)
        {
            services.AddOptions<GaitAnalysisOptions>();
            services.AddOptions<SensorAlignmentOptions>();
            services.AddOptions<ReportOptions>();
        }
        else
        {
            services.Configure<GaitAnalysisOptions>(configuration.GetSection("GaitAnalysis"));
            services.Configure<SensorAlignmentOptions>(configuration.GetSection("SensorAlignment"));
            services.Configure<ReportOptions>(configuration.GetSection("Reports"));
        }

        services.AddSingleton<IFootScanParser, FootScanParser>();
        services.AddSingleton<IHeatmapFeatureExtractor, HeatmapFeatureExtractor>();
        services.AddSingleton<IFootRiskClassifier, FootRiskClassifier>();
        services.AddSingleton<IFootAnalysisService, FootAnalysisService>();
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<GaitAnalysisOptions>>().Value);
        services.AddSingleton<IGaitAnalysisService, OnnxGaitAnalysisService>();
        services.AddSingleton<ISensorAlignmentService, DynamicTimeWarpingSensorAlignmentService>();

        return services;
    }
}
