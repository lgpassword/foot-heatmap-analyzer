# Foot Heatmap Analyzer

Foot Heatmap Analyzer is an open-source ASP.NET Core demo for parsing binary plantar pressure or temperature scans, rendering left and right foot heatmaps, and producing transparent auxiliary screening signals.

This project is intended for research, education, prototyping, and open-source collaboration. It is not a medical device and does not provide diagnosis.

## Features

- ASP.NET Core Razor Pages web interface.
- Import `.bin`, `.dat`, `.hex`, `.txt`, `.b64`, and `.base64` scan files or paste hex, bit-string, or Base64 payloads.
- Render left and right foot heatmaps on HTML canvas.
- Produce non-diagnostic screening output for:
  - arch type tendency
  - gait loading pattern
  - center-of-pressure balance
  - diabetic foot early screening signals
  - foot deformity and skeletal risk signals
  - rehabilitation and orthotic review hints
  - neurological asymmetry signals
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
- `FootAnalysisService` in `Core`: orchestrates extraction and classification into the final report through interfaces.
- `AddFootHeatmapAnalyzer` in `Composition`: registers the parser, algorithms, classifier, and analysis service in one place.

## Medical Disclaimer

The analysis output is screening-oriented and non-diagnostic. It should not be used to diagnose diabetes, neuropathy, skeletal disease, gait disease, or any other medical condition. Clinical use requires validated sensors, calibrated acquisition, clinical datasets, regulatory review, and evaluation by qualified medical professionals.

## License

MIT
