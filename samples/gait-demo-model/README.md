# Gait Demo Model

本目录不随仓库提供真实 ONNX 模型。`/api/gait/analyze` 在未配置模型时会返回 `ModelNotConfigured` 占位结果，便于接口和前端流程继续工作。

要启用真实云端步态识别，请在 `src/FootHeatmapAnalyzer.Web/appsettings.json` 的 `GaitAnalysis:ModelPath` 中配置 `.onnx` 文件路径。模型输入约定为：

```text
float32[1, N]
```

其中 `N` 是压力序列展平后的特征向量长度。可以从 PyTorch、scikit-learn 或其他训练框架导出 ONNX 文件，并确保输出张量为 4 个类别概率：

- `Normal`
- `Pronation`
- `Supination`
- `Antalgic`

PyTorch 导出示例：

```python
torch.onnx.export(
    model,
    sample_tensor,  # shape: [1, N]
    "gait-demo.onnx",
    input_names=["pressure_sequence"],
    output_names=["probabilities"],
    opset_version=17,
)
```

scikit-learn 可通过 `skl2onnx` 转换分类器，转换时保持输入名为 `pressure_sequence`，输出名为 `probabilities`。
