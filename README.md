# Cocodif

Diff-coverage reporter for .NET. Parses **OpenCover** or **Cobertura** XML coverage data, computes coverage only for changed files, and emits Markdown or JSON reports.

## Quick Start

**GitHub Action** — add one step to your workflow:

```yaml
- name: Coverage report
  uses: sanet/Cocodif@v1
  with:
    coverage-files: '**/coverage.opencover.xml'
    fail-under: '80'
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

**CLI** — install and run directly:

```bash
dotnet tool install --global Sanet.Cocodif
Cocodif -c coverage.xml -f changed-files.txt -o report.md
```

## Components

| Component | Path | Description |
|-----------|------|-------------|
| **Cocodif CLI** | [`src/Cocodif/`](src/Cocodif/) | .NET global tool — the core engine |
| **GitHub Action** | [`action/`](action/) | Composite action that wraps the CLI for GitHub Actions |

Each component has its own README with detailed usage, inputs, and examples:

- [`src/Cocodif/README.md`](src/Cocodif/README.md) — CLI options, exit codes, output formats
- [`action/README.md`](action/README.md) — Action inputs/outputs, permissions, workflow examples

## Project Structure

```
Cocodif/
├── src/Cocodif/          # CLI tool (NuGet package: Sanet.Cocodif)
│   ├── Parsers/           # OpenCover & Cobertura XML parsers
│   ├── Reporters/         # Markdown & JSON reporters
│   ├── Services/          # Path normalization, diff logic, coverage merging
│   └── Models/            # Data models
├── tests/Cocodif.Tests/   # xUnit tests
├── action/                # GitHub Action (composite)
│   ├── action.yml
│   └── README.md
└── Cocodif.sln
```

## Requirements

- .NET 10.0 SDK (for the CLI tool)
- For the GitHub Action: `fetch-depth: 0` on `actions/checkout` when using automatic diff detection

## License

[MIT](LICENSE)
