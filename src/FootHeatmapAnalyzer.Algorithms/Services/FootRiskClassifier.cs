using FootHeatmapAnalyzer.Core.Models;
using FootHeatmapAnalyzer.Core.Services;

namespace FootHeatmapAnalyzer.Algorithms.Services;

/// <summary>
/// Converts extracted heatmap features into transparent non-diagnostic screening text.
/// </summary>
public sealed class FootRiskClassifier(AnalysisOptions? options = null) : IFootRiskClassifier
{
    // Holds all thresholds used by the rule-based classifier.
    private readonly AnalysisOptions options = options ?? new AnalysisOptions();

    public string ClassifyArch(FootScanMetrics metrics)
    {
        return metrics.CombinedArchIndex switch
        {
            var value when value < options.HighArchUpperBound => "高足弓倾向",
            var value when value > options.LowArchLowerBound => "低足弓或扁平足倾向",
            _ => "中性足弓倾向"
        };
    }

    public string DescribeGait(FootScanMetrics metrics)
    {
        return metrics.ForeHeelAsymmetry > options.ForeHeelAsymmetryThreshold
            ? $"负载模式不对称，建议复核足跟着地到前足蹬伸的过渡过程（差异 {metrics.ForeHeelAsymmetry:P0}）。"
            : $"当前静态样本的负载模式相对对称（差异 {metrics.ForeHeelAsymmetry:P0}）。";
    }

    public BalanceResult DescribeBalance(FootScanMetrics metrics)
    {
        var state = metrics.LeftLoadShare switch
        {
            var value when value < options.LeftLoadLowerBound => BalanceState.RightHeavy,
            var value when value > options.LeftLoadUpperBound => BalanceState.LeftHeavy,
            _ => BalanceState.Balanced
        };
        var description = state switch
        {
            BalanceState.RightHeavy => "右侧负载偏重",
            BalanceState.LeftHeavy => "左侧负载偏重",
            _ => "左右负载较均衡"
        };
        var label = $"{description}（左侧 {metrics.LeftLoadShare:P0} / 右侧 {1 - metrics.LeftLoadShare:P0}）";

        return new BalanceResult(state, metrics.LeftLoadShare, label);
    }

    public IReadOnlyList<AnalysisFinding> BuildFindings(FootScanMetrics metrics, BalanceResult balance)
    {
        return
        [
            BuildHotspotFinding(metrics),
            BuildArchContactFinding(metrics),
            BuildRehabilitationFinding(metrics),
            BuildBalanceFinding(metrics, balance),
            BuildContactQualityFinding(metrics)
        ];
    }

    public FootAnalysisReport BuildReport(FootScanMetrics metrics, string disclaimer)
    {
        var archType = ClassifyArch(metrics);
        var balance = DescribeBalance(metrics);
        var gait = DescribeGait(metrics);
        var findings = BuildFindings(metrics, balance);

        return new FootAnalysisReport(archType, gait, balance.Label, findings, disclaimer)
        {
            ForefootHeelRatio = new(
                CalculateForefootHeelRatio(metrics.Left),
                CalculateForefootHeelRatio(metrics.Right)),
            ArchIndexCategory = new(
                ClassifyArchIndex(metrics.Left.ArchIndex),
                ClassifyArchIndex(metrics.Right.ArchIndex)),
            LoadSymmetryScore = CalculateLoadSymmetryScore(metrics),
            HotspotSeverity = new(
                ClassifyHotspotSeverity(metrics.Left.HotspotCount),
                ClassifyHotspotSeverity(metrics.Right.HotspotCount))
        };
    }

    private AnalysisFinding BuildHotspotFinding(FootScanMetrics metrics)
    {
        var elevated = metrics.PeakPressure > options.HotspotThreshold || metrics.HotspotCount >= options.HotspotCountReviewThreshold;
        return new AnalysisFinding(
            "局部高压热点统计",
            elevated ? "需复核" : "观察",
            elevated ? $"检测到局部高强度压力点（{metrics.HotspotCount} 个热点单元）。" : "当前演示扫描中未发现极端热点聚集。",
            "持续存在的局部高压需要结合传感器校准、重复测量和专业人员判断。");
    }

    private AnalysisFinding BuildArchContactFinding(FootScanMetrics metrics)
    {
        var flatfootSignal = metrics.CombinedArchIndex > options.LowArchLowerBound && metrics.ContactAreaRatio > options.LowArchContactThreshold;
        var highArchSignal = metrics.CombinedArchIndex < options.HighArchUpperBound && metrics.ContactAreaRatio < options.HighArchContactThreshold;
        return new AnalysisFinding(
            "足弓与接触面积分布",
            flatfootSignal || highArchSignal ? "需复核" : "较低",
            flatfootSignal ? "中足接触面积偏大，呈现低足弓负载模式。" : highArchSignal ? "中足接触面积偏低，呈现高足弓负载模式。" : "足弓指数和接触面积处于演示中性区间。",
            "足弓指数结合接触面积，比单独查看中足强度更稳定。");
    }

    private AnalysisFinding BuildRehabilitationFinding(FootScanMetrics metrics)
    {
        var foreHeelGap = Math.Abs(((metrics.Left.ForefootLoad + metrics.Right.ForefootLoad) / 2) - ((metrics.Left.HeelLoad + metrics.Right.HeelLoad) / 2));
        return new AnalysisFinding(
            "前足/足跟负载分布",
            foreHeelGap > options.ForeHeelGapThreshold || metrics.HotspotCount > 0 ? "需复核" : "持续跟踪",
            foreHeelGap > options.ForeHeelGapThreshold ? $"前足与足跟负载差异较明显（{foreHeelGap:P0}）。" : "当前样本前足与足跟负载较接近。",
            "区域负载差异和热点分布可用于讨论压力重分配和设备校准方案。");
    }

    private AnalysisFinding BuildBalanceFinding(FootScanMetrics metrics, BalanceResult balance)
    {
        var centerGap = Math.Abs(metrics.Left.CenterX - (1 - metrics.Right.CenterX));
        var review = metrics.AveragePressureAsymmetry > options.AveragePressureAsymmetryThreshold || centerGap > options.CenterGapThreshold || balance.State != BalanceState.Balanced;
        return new AnalysisFinding(
            "左右负载与压力中心对称性",
            review ? "需复核" : "较低",
            review ? $"负载或压力中心特征存在不对称（中心差异 {centerGap:P0}）。" : "当前样本未见明显左右不对称。",
            "不对称负载需要结合重复测量、姿态控制和采集质量判断。");
    }

    private AnalysisFinding BuildContactQualityFinding(FootScanMetrics metrics)
    {
        var sparse = metrics.ContactAreaRatio < options.SparseContactThreshold;
        return new AnalysisFinding(
            "热力图识别质量",
            sparse ? "谨慎" : "可用",
            sparse ? $"仅 {metrics.ContactAreaRatio:P0} 的单元处于有效接触状态，扫描覆盖可能偏稀疏。" : $"接触覆盖率为 {metrics.ContactAreaRatio:P0}，可用于当前演示规则。",
            "识别可信度取决于传感器覆盖、校准质量和重复测量稳定性。");
    }

    private static double CalculateForefootHeelRatio(FootRegionMetrics metrics)
    {
        return Math.Round(Math.Clamp(metrics.ForefootLoad / Math.Max(metrics.HeelLoad, .001), .1, 10), 3);
    }

    private static string ClassifyArchIndex(double archIndex)
    {
        return archIndex switch
        {
            < .21 => "High",
            <= .26 => "Normal",
            _ => "Flat"
        };
    }

    private static double CalculateLoadSymmetryScore(FootScanMetrics metrics)
    {
        var total = metrics.Left.TotalLoad + metrics.Right.TotalLoad;
        return Math.Round(Math.Clamp(1 - (Math.Abs(metrics.Left.TotalLoad - metrics.Right.TotalLoad) / Math.Max(total, .001)), 0, 1), 3);
    }

    private static string ClassifyHotspotSeverity(int count)
    {
        return count switch
        {
            0 => "None",
            <= 2 => "Mild",
            <= 4 => "Moderate",
            _ => "High"
        };
    }
}
