using FootHeatmapAnalyzer.Composition;
using FootHeatmapAnalyzer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootHeatmapAnalyzer.Tests;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddFootHeatmapAnalyzer_RegistersCoreServices()
    {
        var services = new ServiceCollection();

        using var provider = services.AddFootHeatmapAnalyzer().BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IFootScanParser>());
        Assert.NotNull(provider.GetRequiredService<IHeatmapFeatureExtractor>());
        Assert.NotNull(provider.GetRequiredService<IFootRiskClassifier>());
        Assert.NotNull(provider.GetRequiredService<IFootAnalysisService>());
    }
}
