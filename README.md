# Cocodif

Diff-coverage reporter for .NET. Parses **OpenCover** or **Cobertura** XML coverage data, computes coverage only for changed files, and emits Markdown or JSON reports.

## Quick Start

**GitHub Action** — add one step to your workflow:

```yaml
- name: Coverage report
  uses: anton-makarevich/Cocodif@v1
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

## GitHub Action

A composite GitHub Action that generates diff-coverage reports and posts them as PR comments or step summaries.

### Action Inputs

| Input | Required | Default | Description |
|-------|----------|---------|-------------|
| `coverage-files` | Yes | - | Glob pattern or comma-separated list of coverage XML file paths |
| `changed-files` | No | `''` | Path to file listing changed files. If omitted, computed via `git diff`. |
| `format` | No | `auto` | Coverage format: `auto`, `opencover`, or `cobertura` |
| `include` | No | `**/*` | Glob pattern for files to include |
| `exclude` | No | `**/obj/**,**/bin/**` | Glob pattern for files to exclude |
| `title` | No | `Coverage Report` | Report title |
| `output-format` | No | `markdown` | Output format: `markdown`, `json`, or `both` |
| `fail-under` | No | `''` | Fail if overall diff coverage is below this percentage |
| `comment` | No | `true` | Post the report as a PR comment |
| `comment-marker` | No | `<!-- cocodif-coverage -->` | HTML comment marker for sticky PR comment |
| `summary` | No | `true` | Write the report to `$GITHUB_STEP_SUMMARY` |
| `cocodif-version` | No | `0.1.0` | Version of the Cocodif NuGet tool to install |

### Action Outputs

| Output | Description |
|--------|-------------|
| `coverage-percent` | Overall diff-coverage percentage |
| `report-path` | Path to the generated report file |

### Required Permissions

```yaml
permissions:
  pull-requests: write
  contents: read
```

### Consumer Workflow Example

```yaml
name: CI

on:
  pull_request:
    branches: [main]

permissions:
  pull-requests: write
  contents: read

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Required for git diff merge-base

      - name: Run tests with coverage
        run: dotnet test --collect:"XPlat Code Coverage"

      - name: Coverage report
        uses: anton-makarevich/Cocodif@v1
        with:
          coverage-files: '**/coverage.opencover.xml'
          fail-under: '80'
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## CLI

The .NET global tool — the core engine.

```bash
dotnet tool install --global Sanet.Cocodif
```

For CLI options, exit codes, and output formats, see [`src/Cocodif/README.md`](src/Cocodif/README.md).

## Project Structure

```
Cocodif/
├── src/Cocodif/          # CLI tool (NuGet package: Sanet.Cocodif)
│   ├── Parsers/           # OpenCover & Cobertura XML parsers
│   ├── Reporters/         # Markdown & JSON reporters
│   ├── Services/          # Path normalization, diff logic, coverage merging
│   └── Models/            # Data models
├── tests/Cocodif.Tests/   # xUnit tests
├── action.yml             # GitHub Action definition (composite)
└── Cocodif.sln
```

## Requirements

- .NET 10.0 SDK (for the CLI tool)
- For the GitHub Action: `fetch-depth: 0` on `actions/checkout` when using automatic diff detection

## License

[MIT](LICENSE)
