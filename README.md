# Foot Heatmap Analyzer

Foot Heatmap Analyzer is an open-source ASP.NET Core demo for parsing binary plantar pressure or temperature scans, rendering left and right foot heatmaps, and producing transparent auxiliary screening signals.

This project is intended for research, education, prototyping, and open-source collaboration. It is not a medical device and does not provide diagnosis.

## Features

- ASP.NET Core Razor Pages web interface.
- Upload a binary scan file or paste hex, bit-string, or Base64 payloads.
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
src/FootHeatmapAnalyzer.Web/
  Models/       Domain records for heatmaps and reports
  Services/     Binary parser and heuristic analysis logic
  Pages/        Razor Pages UI
tests/FootHeatmapAnalyzer.Tests/
  Parser and analysis unit tests
```

## Medical Disclaimer

The analysis output is screening-oriented and non-diagnostic. It should not be used to diagnose diabetes, neuropathy, skeletal disease, gait disease, or any other medical condition. Clinical use requires validated sensors, calibrated acquisition, clinical datasets, regulatory review, and evaluation by qualified medical professionals.

## License

MIT
