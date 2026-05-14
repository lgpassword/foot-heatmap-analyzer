# Foot Heatmap Analyzer

语言：[English](README.md) | 简体中文

Foot Heatmap Analyzer 是一个开源 ASP.NET Core 演示项目，用于解析二进制足底压力或温度扫描数据，渲染左右足热力图，并输出透明、可解释的辅助筛查信号。

本项目适用于研究、教学、原型验证和开源协作。它不是医疗器械，也不提供诊断结论。

算法、数据流和工程边界说明见：[算法与工程说明](docs/ALGORITHMS.zh-CN.md)。

## 实现状态

| 组件 | 状态 |
|------|------|
| 二进制解析器 | 已完成 |
| 热力图特征提取 | 已完成 |
| 风险分类器 | 已完成 |
| DTW 传感器对齐 | 已完成 (v0.2) |
| ONNX 步态分析 | 集成点 - 需提供自有模型 |
| QuestPDF 报告 | 已完成 (v0.4) |
| ECharts 仪表盘 | 已完成 (v0.4) |
| SignalR 实时 Hub | Hub 已注册 - 客户端集成可选 |
| 患者档案 | 仅内存存储 - 生产环境需替换 |

## 功能

- ASP.NET Core Razor Pages Web 界面。
- 支持导入 `.bin`、`.dat`、`.hex`、`.txt`、`.b64` 和 `.base64` 扫描文件，也支持粘贴十六进制、二进制位串或 Base64 数据。
- 使用 HTML canvas 渲染左足和右足热力图。
- 输出非诊断性的筛查信息，包括：
  - 足弓类型倾向
  - 步态负载模式
  - 压力中心平衡
  - 局部热点统计
  - 足弓与接触面积分布
  - 前足/足跟负载分布
  - 左右负载与压力中心对称性
- 浏览器端 WebGL 热力图渲染，支持双三次插值和紧凑 raw pressure 帧。
- 服务端 ONNX Runtime 集成点，用于云端压力序列步态识别。
- 使用动态时间规整（DTW）对齐压力数据和手机加速度计时间序列。
- 基于 ASP.NET Core Identity 边界的多租户患者/运动员档案管理。
- 面向外部压力感应硬件的开放 REST 接入 API。
- 面向 ECharts 的仪表盘数据，包括 CoP 偏移、左右脚受力平衡、前足/足跟分布和热点数量。
- 基于 QuestPDF 的可打印足底压力 PDF 报告生成。
- 无数据库，不做持久化存储。
- 包含解析和分析服务的单元测试。

## 程序优势

- 解析、特征提取、规则筛查、可视化和报告生成分层实现，后续替换硬件协议或模型时影响范围较小。
- 热力图渲染优先使用浏览器 WebGL，后端只传输紧凑数据，适合继续扩展实时数据流。
- 筛查规则保持透明，输出字段可以追溯到压力矩阵统计值，便于调试和二次开发。
- Dashboard 和 PDF 共用同一套分析结果，减少不同展示入口之间的数据差异。
- ONNX 步态分析以集成点形式存在，未配置模型时明确返回占位结果，不伪造预测结论。

## 二进制输入格式

当前演示协议刻意保持简单：

```text
byte 0: 宽度
byte 1: 高度
接下来的 width * height 字节: 左足矩阵值，范围 0-255
再接下来的 width * height 字节: 右足矩阵值，范围 0-255
```

示例数据可以用十六进制、Base64 或连续二进制位串粘贴。

`samples/` 目录提供了可直接使用的合成参考扫描数据，包括 `.hex`、`.bin` 和 CSV 矩阵格式。

## 文件导入

Web 表单支持以下文件类型：

- `.bin` 和 `.dat`：按原始协议字节解析。
- `.hex` 和 `.txt`：按文本解析，可包含十六进制、二进制位串或 Base64。
- `.b64` 和 `.base64`：按 Base64 文本解析。

文件只在内存中处理，不会保存到磁盘。

## API 使用

Web 主机还提供最小 JSON API，与 Razor UI 复用同一套解析器和分析服务：

```bash
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/octet-stream" \
  --data-binary "@samples/sample-scan.bin"

curl -X POST http://localhost:5000/api/analyze/text \
  -H "Content-Type: text/plain" \
  --data-binary "@samples/sample-scan.hex"

curl -X POST http://localhost:5000/api/render-frame/text \
  -H "Content-Type: text/plain" \
  --data-binary "@samples/sample-scan.hex"

curl -X POST http://localhost:5000/api/hardware/scans \
  -H "Content-Type: application/json" \
  -d '{"deviceId":"demo-device","tenantId":"demo","profileId":"athlete-1","payloadEncoding":"hex","payload":"<HEX_PAYLOAD>","capturedAt":"2026-05-14T00:00:00Z"}'

curl -X POST http://localhost:5000/api/dashboard \
  -H "Content-Type: text/plain" \
  --data-binary "@samples/sample-scan.hex"

curl -X POST http://localhost:5000/api/reports/pdf \
  -H "Content-Type: text/plain" \
  --data-binary "@samples/sample-scan.hex" \
  --output foot-pressure-report.pdf
```

无效载荷会返回 HTTP 400，并附带 ProblemDetails 响应。

`/api/render-frame` 和 `/api/render-frame/text` 返回紧凑的 base64 raw pressure 阵列，供浏览器端 GPU 或 Canvas 渲染使用。响应会声明双三次插值，让客户端本地渲染热力图，而不是依赖服务端生成图片。SignalR 客户端也可以连接 `/hubs/heatmap`，接收用于实时渲染的 `heatmapFrame` 载荷。

`/api/gait/analyze` 接收带时间戳的压力序列特征向量或扫描载荷，并通过 `FootHeatmapAnalyzer.GaitAnalysis` 执行步态分类。如果已配置 ONNX 模型路径且文件存在，则使用 ONNX Runtime；否则返回 `ModelNotConfigured` 占位结果和配置说明。

`/api/sensors/align` 接收压力负载样本和手机加速度计样本，并通过 `FootHeatmapAnalyzer.SensorAlignment` 使用完整 DTW 代价矩阵和单调回溯路径，对非等频采样时间序列进行对齐。

`/api/profiles` 管理租户隔离的患者或运动员档案。当前演示从 Identity claims 或 `X-Tenant-Id` 解析租户；生产部署应把内存存储替换为持久化数据库。

`/api/hardware/scans` 是开放硬件接入端点。设备提交元数据以及十六进制或 Base64 压力载荷，平台返回分析报告、渲染帧和仪表盘数据。

`/api/dashboard` 返回 ECharts 可直接使用的图表数据。`/api/reports/pdf` 返回 QuestPDF 生成的可打印 PDF 报告。

## 本地运行

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\FootHeatmapAnalyzer.Web
```

然后打开 ASP.NET Core 在终端中输出的本地地址。

## 项目结构

```text
src/FootHeatmapAnalyzer.Core/
  Models/       热力图、指标和报告相关领域模型
  Services/     核心解析器、服务契约和报告编排
src/FootHeatmapAnalyzer.Algorithms/
  Services/     热力图特征提取和筛查分类器
src/FootHeatmapAnalyzer.Composition/
  *.cs          统一依赖注入注册
src/FootHeatmapAnalyzer.GaitAnalysis/
  Services/     ONNX Runtime 步态序列识别
src/FootHeatmapAnalyzer.SensorAlignment/
  Services/     动态时间规整传感器数据对齐
src/FootHeatmapAnalyzer.UiStyles/
  wwwroot/      共享 CSS 和 JavaScript 静态资源
src/FootHeatmapAnalyzer.Web/
  Pages/        Razor Pages UI 和上传处理
tests/FootHeatmapAnalyzer.Tests/
  解析器、算法和服务注册测试
```

## 识别流程

当前识别流程保持模块化：

- `Core` 中的 `FootScanParser`：将字节或粘贴的载荷解析为归一化热力图矩阵。
- `Algorithms` 中的 `HeatmapFeatureExtractor`：提取区域负载、足弓指数、接触面积、热点和压力中心特征。
- `Algorithms` 中的 `FootRiskClassifier`：将特征转换为透明、非诊断性的筛查分类。
- `GaitAnalysis` 中的 `OnnxGaitAnalysisService`：在服务端运行训练好的 1D-CNN、Transformer 或类似 ONNX 序列模型。
- `SensorAlignment` 中的 `DynamicTimeWarpingSensorAlignmentService`：对不同采样频率的压力和加速度计数据流进行时间轴对齐。
- `Core` 中的 `FootAnalysisService`：通过接口编排特征提取和分类，生成最终报告。
- `Composition` 中的 `AddFootHeatmapAnalyzer`：统一注册解析器、算法、分类器和分析服务。

## 医疗免责声明

分析输出描述的是可观察的热力图特征，例如热点、接触分布、足弓指数和左右负载平衡。它面向筛查演示，不具有诊断用途。它不应被用于诊断糖尿病、神经病变、骨骼疾病、步态疾病或任何其他医疗状况。临床使用需要经过验证的传感器、校准采集流程、临床数据集、监管审查，并由合格医疗专业人员评估。

## 许可证

MIT
