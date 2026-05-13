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
            < .20 => "高足弓倾向",
            > .31 => "低足弓或扁平足倾向",
            _ => "中性足弓倾向"
        };
    }

    public string DescribeGait(FootScanMetrics metrics)
    {
        return metrics.ForeHeelAsymmetry > .18
            ? $"负载模式不对称，建议复核足跟着地到前足蹬伸的过渡过程（差异 {metrics.ForeHeelAsymmetry:P0}）。"
            : $"当前静态样本的负载模式相对对称（差异 {metrics.ForeHeelAsymmetry:P0}）。";
    }

    public string DescribeBalance(FootScanMetrics metrics)
    {
        var label = metrics.LeftLoadShare switch
        {
            < .43 => "右侧负载偏重",
            > .57 => "左侧负载偏重",
            _ => "左右负载较均衡"
        };

        return $"{label}（左侧 {metrics.LeftLoadShare:P0} / 右侧 {1 - metrics.LeftLoadShare:P0}）";
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
            "糖尿病足早期筛查",
            elevated ? "偏高" : "观察",
            elevated ? $"检测到局部高强度压力点（{metrics.HotspotCount} 个热点单元）。" : "当前演示扫描中未发现极端热点聚集。",
            "持续存在的局部高压可能提示溃疡风险，需要结合临床情况复核。");
    }

    private static AnalysisFinding BuildDeformityFinding(FootScanMetrics metrics)
    {
        var flatfootSignal = metrics.CombinedArchIndex > .31 && metrics.ContactAreaRatio > .58;
        var highArchSignal = metrics.CombinedArchIndex < .20 && metrics.ContactAreaRatio < .48;
        return new AnalysisFinding(
            "足部畸形与骨骼风险",
            flatfootSignal || highArchSignal ? "需复核" : "较低",
            flatfootSignal ? "中足接触面积偏大，提示可能存在扁平足负载模式。" : highArchSignal ? "中足接触面积偏低，提示可能存在高足弓负载模式。" : "足弓指数和接触面积处于演示中性区间。",
            "足弓指数结合接触面积，比单独查看中足强度更稳定。");
    }

    private static AnalysisFinding BuildRehabilitationFinding(FootScanMetrics metrics)
    {
        var foreHeelGap = Math.Abs(((metrics.Left.ForefootLoad + metrics.Right.ForefootLoad) / 2) - ((metrics.Left.HeelLoad + metrics.Right.HeelLoad) / 2));
        return new AnalysisFinding(
            "康复评估与矫形建议",
            foreHeelGap > .22 || metrics.HotspotCount > 0 ? "需复核" : "持续跟踪",
            foreHeelGap > .22 ? $"前足与足跟负载差异较明显（{foreHeelGap:P0}）。" : "当前样本前足与足跟负载较接近。",
            "区域负载差异和热点分布可用于讨论压力重分配和矫形方案。");
    }

    private static AnalysisFinding BuildNeurologicalFinding(FootScanMetrics metrics, string balance)
    {
        var centerGap = Math.Abs(metrics.Left.CenterX - (1 - metrics.Right.CenterX));
        var review = metrics.AveragePressureAsymmetry > .12 || centerGap > .18 || !balance.StartsWith("左右");
        return new AnalysisFinding(
            "神经系统疾病信号提示",
            review ? "需复核" : "较低",
            review ? $"负载或压力中心特征存在不对称（中心差异 {centerGap:P0}）。" : "当前样本未见明显左右不对称。",
            "不对称负载可能与代偿、肌力变化或感觉反馈变化有关。");
    }

    private static AnalysisFinding BuildContactQualityFinding(FootScanMetrics metrics)
    {
        var sparse = metrics.ContactAreaRatio < .35;
        return new AnalysisFinding(
            "热力图识别质量",
            sparse ? "谨慎" : "可用",
            sparse ? $"仅 {metrics.ContactAreaRatio:P0} 的单元处于有效接触状态，扫描覆盖可能偏稀疏。" : $"接触覆盖率为 {metrics.ContactAreaRatio:P0}，可用于当前演示规则。",
            "识别可信度取决于传感器覆盖、校准质量和重复测量稳定性。");
    }
}
