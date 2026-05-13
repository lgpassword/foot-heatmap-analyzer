using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Implements transparent heuristic screening metrics for demo heatmap analysis.
/// </summary>
public sealed class FootAnalysisService : IFootAnalysisService
{
    private const string Disclaimer = "This project provides screening-oriented, non-diagnostic research output. It does not diagnose disease or replace clinical evaluation.";

    public FootAnalysisReport Analyze(ParsedFootScan scan)
    {
        var combinedMidfoot = (RegionAverage(scan.LeftFoot, .35, .70) + RegionAverage(scan.RightFoot, .35, .70)) / 2;
        var combinedForefoot = (RegionAverage(scan.LeftFoot, 0, .35) + RegionAverage(scan.RightFoot, 0, .35)) / 2;
        var combinedHeel = (RegionAverage(scan.LeftFoot, .70, 1) + RegionAverage(scan.RightFoot, .70, 1)) / 2;
        var archType = ClassifyArch(combinedMidfoot, combinedForefoot, combinedHeel);
        var balance = DescribeBalance(scan);
        var gait = DescribeGait(scan);
        var findings = BuildFindings(scan, combinedMidfoot, combinedForefoot, combinedHeel, balance);

        return new FootAnalysisReport(archType, gait, balance, findings, Disclaimer);
    }

    private static string ClassifyArch(double midfoot, double forefoot, double heel)
    {
        var supportAverage = Math.Max((forefoot + heel) / 2, .01);
        var midfootRatio = midfoot / supportAverage;
        return midfootRatio switch
        {
            < .42 => "High arch tendency",
            > .78 => "Low arch / flatfoot tendency",
            _ => "Neutral arch tendency"
        };
    }

    private static string DescribeGait(ParsedFootScan scan)
    {
        var leftForefoot = RegionAverage(scan.LeftFoot, 0, .35);
        var leftHeel = RegionAverage(scan.LeftFoot, .70, 1);
        var rightForefoot = RegionAverage(scan.RightFoot, 0, .35);
        var rightHeel = RegionAverage(scan.RightFoot, .70, 1);
        var propulsionGap = Math.Abs((leftForefoot - leftHeel) - (rightForefoot - rightHeel));

        return propulsionGap > .18
            ? "Asymmetric loading pattern; review heel-strike to toe-off transition."
            : "Relatively symmetric loading pattern in this static sample.";
    }

    private static string DescribeBalance(ParsedFootScan scan)
    {
        var leftTotal = scan.LeftFoot.AllValues().Sum();
        var rightTotal = scan.RightFoot.AllValues().Sum();
        var total = Math.Max(leftTotal + rightTotal, .01);
        var leftShare = leftTotal / total;
        var label = leftShare switch
        {
            < .43 => "right-side load bias",
            > .57 => "left-side load bias",
            _ => "balanced left/right loading"
        };

        return $"{label} ({leftShare:P0} left / {1 - leftShare:P0} right)";
    }

    private static IReadOnlyList<AnalysisFinding> BuildFindings(ParsedFootScan scan, double midfoot, double forefoot, double heel, string balance)
    {
        var peak = Math.Max(scan.LeftFoot.AllValues().Max(), scan.RightFoot.AllValues().Max());
        var asymmetry = Math.Abs(scan.LeftFoot.AllValues().Average() - scan.RightFoot.AllValues().Average());
        var foreHeelGap = Math.Abs(forefoot - heel);

        return
        [
            new("Diabetic foot early screening", peak > .88 ? "Elevated" : "Watch", peak > .88 ? "Localized high-intensity points detected." : "No extreme hotspot in the demo scan.", "Persistent hotspots can indicate pressure concentration and should be reviewed clinically."),
            new("Foot deformity and skeletal risk", midfoot > .62 ? "Watch" : "Low", midfoot > .62 ? "Broad midfoot contact suggests possible flatfoot loading." : "Midfoot loading is not broadly elevated.", "Arch-related load distribution can correlate with deformity or alignment concerns."),
            new("Rehabilitation and orthotic guidance", foreHeelGap > .22 ? "Review" : "Track", foreHeelGap > .22 ? "Forefoot and heel load differ materially." : "Forefoot and heel load are close in this sample.", "Large regional differences may guide orthotic pressure redistribution discussions."),
            new("Neurological signal screening", asymmetry > .12 || !balance.StartsWith("balanced") ? "Review" : "Low", asymmetry > .12 ? "Left/right average load asymmetry detected." : "No strong left/right asymmetry in this sample.", "Asymmetric loading may reflect compensation, weakness, or sensory feedback changes.")
        ];
    }

    private static double RegionAverage(FootHeatmap foot, double startRatio, double endRatio)
    {
        var start = Math.Clamp((int)Math.Floor(foot.Height * startRatio), 0, foot.Height - 1);
        var end = Math.Clamp((int)Math.Ceiling(foot.Height * endRatio), start + 1, foot.Height);
        var values = foot.Values.Skip(start).Take(end - start).SelectMany(row => row);
        return values.DefaultIfEmpty(0).Average();
    }
}
