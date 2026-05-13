using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

namespace FootHeatmapAnalyzer.Algorithms.Services;

/// <summary>
/// Converts extracted heatmap features into transparent non-diagnostic screening text.
/// </summary>
public sealed class FootRiskClassifier : IFootRiskClassifier
{
    public string ClassifyArch(FootScanMetrics metrics)
    {
        return metrics.CombinedArchIndex switch
        {
            < .20 => "High arch tendency",
            > .31 => "Low arch / flatfoot tendency",
            _ => "Neutral arch tendency"
        };
    }

    public string DescribeGait(FootScanMetrics metrics)
    {
        return metrics.ForeHeelAsymmetry > .18
            ? $"Asymmetric loading pattern; review heel-strike to toe-off transition (gap {metrics.ForeHeelAsymmetry:P0})."
            : $"Relatively symmetric loading pattern in this static sample (gap {metrics.ForeHeelAsymmetry:P0}).";
    }

    public string DescribeBalance(FootScanMetrics metrics)
    {
        var label = metrics.LeftLoadShare switch
        {
            < .43 => "right-side load bias",
            > .57 => "left-side load bias",
            _ => "balanced left/right loading"
        };

        return $"{label} ({metrics.LeftLoadShare:P0} left / {1 - metrics.LeftLoadShare:P0} right)";
    }

    public IReadOnlyList<AnalysisFinding> BuildFindings(FootScanMetrics metrics, string balance)
    {
        return
        [
            BuildDiabeticFootFinding(metrics),
            BuildDeformityFinding(metrics),
            BuildRehabilitationFinding(metrics),
            BuildNeurologicalFinding(metrics, balance),
            BuildContactQualityFinding(metrics)
        ];
    }

    private static AnalysisFinding BuildDiabeticFootFinding(FootScanMetrics metrics)
    {
        var elevated = metrics.PeakPressure > .88 || metrics.HotspotCount >= 3;
        return new AnalysisFinding(
            "Diabetic foot early screening",
            elevated ? "Elevated" : "Watch",
            elevated ? $"Localized high-intensity pressure detected ({metrics.HotspotCount} hotspot cells)." : "No extreme hotspot cluster in the demo scan.",
            "Persistent focal pressure can indicate ulceration risk and should be reviewed clinically.");
    }

    private static AnalysisFinding BuildDeformityFinding(FootScanMetrics metrics)
    {
        var flatfootSignal = metrics.CombinedArchIndex > .31 && metrics.ContactAreaRatio > .58;
        var highArchSignal = metrics.CombinedArchIndex < .20 && metrics.ContactAreaRatio < .48;
        return new AnalysisFinding(
            "Foot deformity and skeletal risk",
            flatfootSignal || highArchSignal ? "Review" : "Low",
            flatfootSignal ? "Broad midfoot contact suggests possible flatfoot loading." : highArchSignal ? "Low midfoot contact suggests possible high-arch loading." : "Arch and contact area are within the demo neutral band.",
            "Arch index plus contact area gives a clearer signal than midfoot intensity alone.");
    }

    private static AnalysisFinding BuildRehabilitationFinding(FootScanMetrics metrics)
    {
        var foreHeelGap = Math.Abs(((metrics.Left.ForefootLoad + metrics.Right.ForefootLoad) / 2) - ((metrics.Left.HeelLoad + metrics.Right.HeelLoad) / 2));
        return new AnalysisFinding(
            "Rehabilitation and orthotic guidance",
            foreHeelGap > .22 || metrics.HotspotCount > 0 ? "Review" : "Track",
            foreHeelGap > .22 ? $"Forefoot and heel load differ materially ({foreHeelGap:P0})." : "Forefoot and heel load are close in this sample.",
            "Regional load gaps and hotspots can guide pressure redistribution discussions.");
    }

    private static AnalysisFinding BuildNeurologicalFinding(FootScanMetrics metrics, string balance)
    {
        var centerGap = Math.Abs(metrics.Left.CenterX - (1 - metrics.Right.CenterX));
        var review = metrics.AveragePressureAsymmetry > .12 || centerGap > .18 || !balance.StartsWith("balanced");
        return new AnalysisFinding(
            "Neurological signal screening",
            review ? "Review" : "Low",
            review ? $"Asymmetry detected across load or center-of-pressure features (center gap {centerGap:P0})." : "No strong left/right asymmetry in this sample.",
            "Asymmetric loading may reflect compensation, weakness, or sensory feedback changes.");
    }

    private static AnalysisFinding BuildContactQualityFinding(FootScanMetrics metrics)
    {
        var sparse = metrics.ContactAreaRatio < .35;
        return new AnalysisFinding(
            "Heatmap recognition quality",
            sparse ? "Caution" : "Usable",
            sparse ? $"Only {metrics.ContactAreaRatio:P0} of cells are active; scan coverage may be too sparse." : $"Contact coverage is {metrics.ContactAreaRatio:P0}, enough for demo heuristics.",
            "Recognition confidence depends on sensor coverage, calibration, and repeatability.");
    }
}
