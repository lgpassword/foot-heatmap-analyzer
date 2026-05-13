# Contributing

Contributions are welcome. Keep changes small, testable, and explicit.

## Development

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\FootHeatmapAnalyzer.Web
```

## Medical Safety

- Do not present outputs as diagnosis.
- Keep clinical claims conservative and traceable to code or documented research.
- Prefer transparent heuristics over opaque conclusions unless a validated model and dataset are added.
- Add tests for parser behavior, matrix calculations, and report generation when changing analysis logic.

## Pull Requests

- Describe the problem and the implementation.
- Include screenshots for UI changes.
- Include test evidence.
- Keep unrelated refactors out of the PR.
