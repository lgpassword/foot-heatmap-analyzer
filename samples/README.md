# Sample Foot Scan Data

This folder contains a deterministic sample scan for local testing and UI demos.

## Files

- `sample-scan.hex`: full payload that can be pasted directly into the web UI.
- `sample-scan.bin`: binary version of the same payload.
- `left-foot.csv`: left foot matrix as raw 0-255 sensor values.
- `right-foot.csv`: right foot matrix as raw 0-255 sensor values.

## Payload Format

```text
byte 0: width = 8
byte 1: height = 12
next 96 bytes: left foot matrix, row-major order
next 96 bytes: right foot matrix, row-major order
```

The web app normalizes each sensor value by dividing by `255`.

## How To Use

Run the app:

```powershell
dotnet run --project src\FootHeatmapAnalyzer.Web
```

Open the local URL and paste the content of `sample-scan.hex` into the input box, then click `Analyze`.

## Notes

This is synthetic demonstration data. It is not captured from a clinical device and must not be used as medical evidence.
