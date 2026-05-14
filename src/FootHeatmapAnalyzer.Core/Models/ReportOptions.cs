namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// 配置 PDF 报告生成时的可选展示内容。
/// </summary>
public sealed class ReportOptions
{
    /// <summary>
    /// 是否在报告中输出原始矩阵摘要页。
    /// </summary>
    public bool IncludeRawDataPage { get; set; } = true;

    /// <summary>
    /// 报告页眉可选 Logo 路径，当前为空时不显示。
    /// </summary>
    public string LogoPath { get; set; } = string.Empty;
}
