# Foot Heatmap Analyzer

Language: English | [简体中文](README.zh-CN.md)

Foot Heatmap Analyzer is an open-source ASP.NET Core demo for parsing binary plantar pressure or temperature scans, rendering left and right foot heatmaps, and producing transparent auxiliary screening signals.

This project is intended for research, education, prototyping, and open-source collaboration. It is not a medical device and does not provide diagnosis.

## Implementation Status

| Component | Status |
|-----------|--------|
| Binary parser | Complete |
| Heatmap feature extraction | Complete |
| Risk classifier | Complete |
| DTW sensor alignment | Complete (v0.2) |
| ONNX gait analysis | Integration point - supply your own model |
| QuestPDF reports | Complete (v0.4) |
| ECharts dashboard | Complete (v0.4) |
| SignalR real-time hub | Hub registered - client integration optional |
| Patient profiles | In-memory only - replace store for production |

## Features

- ASP.NET Core Razor Pages web interface.
- Import `.bin`, `.dat`, `.hex`, `.txt`, `.b64`, and `.base64` scan files or paste hex, bit-string, or Base64 payloads.
- Render left and right foot heatmaps on HTML canvas.
- Produce non-diagnostic screening output for:
  - arch type tendency
  - gait loading pattern
  - center-of-pressure balance
  - local hotspot statistics
  - arch and contact-area distribution
  - forefoot/heel load distribution
  - left-right load and pressure-center symmetry
- Browser-side WebGL heatmap rendering with bicubic interpolation and compact raw pressure frames.
- Server-side ONNX Runtime integration point for cloud-based gait sequence recognition.
- Dynamic Time Warping alignment for pressure and accelerometer time-series uploads.
- Tenant-scoped patient and athlete profile management using ASP.NET Core Identity boundaries.
- Open REST hardware ingestion API for external pressure sensing devices.
- ECharts dashboard payloads for CoP offset, left/right load balance, forefoot/heel distribution, and hotspot counts.
- QuestPDF report generation for printable pressure analysis PDFs.
- No database or persistent storage.
- Unit tests for parsing and analysis services.

## Binary Input Format

The first demo protocol is intentionally simple:

```text
byte 0: width
byte 1: height
next width * height bytes: left foot matrix values, 0-255
next width * height bytes: right foot matrix values, 0-255
```

Example payloads can be pasted as hexadecimal, Base64, or a continuous binary bit string.

See `samples/` for a ready-to-use synthetic reference scan in `.hex`, `.bin`, and CSV matrix formats.

## File Import

The web form accepts these file types:

- `.bin` and `.dat`: parsed as raw protocol bytes.
- `.hex` and `.txt`: parsed as text containing hexadecimal, binary bit strings, or Base64.
- `.b64` and `.base64`: parsed as Base64 text.

Files are processed in memory and are not stored.

## API Usage

The web host also exposes minimal JSON APIs that reuse the same parser and analysis service as the Razor UI:

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

Invalid payloads return HTTP 400 with a ProblemDetails response.

`/api/render-frame` and `/api/render-frame/text` return compact base64-encoded raw pressure arrays for browser-side GPU or Canvas rendering. The payload advertises bicubic interpolation so clients can render locally instead of receiving server-generated images. SignalR clients can also connect to `/hubs/heatmap` and receive `heatmapFrame` payloads for live rendering.

`/api/gait/analyze` accepts timestamped pressure sequence feature vectors or a scan payload and runs gait classification through `FootHeatmapAnalyzer.GaitAnalysis`. If an ONNX model path is configured and the model file exists, ONNX Runtime is used; otherwise the service returns a `ModelNotConfigured` placeholder result with setup guidance.

`/api/sensors/align` accepts pressure load samples and phone accelerometer samples, then aligns non-uniform time series with a full Dynamic Time Warping cost matrix and monotonic backtrace through `FootHeatmapAnalyzer.SensorAlignment`.

`/api/profiles` manages tenant-scoped patient or athlete profiles. The current demo resolves tenants from Identity claims or `X-Tenant-Id`; production deployments should replace the in-memory store with a persistent database.

`/api/hardware/scans` is the open hardware integration endpoint. Devices submit metadata plus hex or Base64 pressure payloads and receive analysis, render-frame, and dashboard responses.

`/api/dashboard` returns chart data for ECharts. `/api/reports/pdf` returns a QuestPDF-generated printable PDF report.

## Run Locally

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\FootHeatmapAnalyzer.Web
```

Then open the local URL printed by ASP.NET Core.

## Project Structure

```text
src/FootHeatmapAnalyzer.Core/
  Models/       Domain records for heatmaps, metrics, and reports
  Services/     Core parser, service contracts, and report orchestration
src/FootHeatmapAnalyzer.Algorithms/
  Services/     Heatmap feature extraction and screening classifiers
src/FootHeatmapAnalyzer.Composition/
  *.cs          Unified dependency injection registration
src/FootHeatmapAnalyzer.GaitAnalysis/
  Services/     ONNX Runtime gait sequence recognition
src/FootHeatmapAnalyzer.SensorAlignment/
  Services/     Dynamic Time Warping sensor stream alignment
src/FootHeatmapAnalyzer.UiStyles/
  wwwroot/      Shared CSS and JavaScript static assets
src/FootHeatmapAnalyzer.Web/
  Pages/        Razor Pages UI and upload handling
tests/FootHeatmapAnalyzer.Tests/
  Parser, algorithm, and service registration tests
```

## Recognition Pipeline

The current recognition pipeline is intentionally modular:

- `FootScanParser` in `Core`: parses bytes or pasted payloads into normalized heatmap matrices.
- `HeatmapFeatureExtractor` in `Algorithms`: extracts region loads, arch index, contact area, hotspots, and center-of-pressure features.
- `FootRiskClassifier` in `Algorithms`: converts features into transparent non-diagnostic screening categories.
- `OnnxGaitAnalysisService` in `GaitAnalysis`: runs trained 1D-CNN, Transformer, or similar ONNX sequence models on the server.
- `DynamicTimeWarpingSensorAlignmentService` in `SensorAlignment`: aligns pressure and accelerometer streams sampled at different rates.
- `FootAnalysisService` in `Core`: orchestrates extraction and classification into the final report through interfaces.
- `AddFootHeatmapAnalyzer` in `Composition`: registers the parser, algorithms, classifier, and analysis service in one place.

## Medical Disclaimer

The analysis output describes observable heatmap features such as hotspots, contact distribution, arch index, and left-right load balance. It is screening-oriented and non-diagnostic. It should not be used to diagnose diabetes, neuropathy, skeletal disease, gait disease, or any other medical condition. Clinical use requires validated sensors, calibrated acquisition, clinical datasets, regulatory review, and evaluation by qualified medical professionals.

## License

MIT
