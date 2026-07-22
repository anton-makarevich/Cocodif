# Cocodif GitHub Action

A composite GitHub Action that generates diff-coverage reports from OpenCover or Cobertura XML coverage data and posts them as PR comments or step summaries.

## Usage

```yaml
- name: Coverage Report
  uses: your-org/Cocodif@v1
  with:
    coverage-files: '**/coverage.opencover.xml'
    fail-under: '80'
    comment: 'true'
    summary: 'true'
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Inputs

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

## Outputs

| Output | Description |
|--------|-------------|
| `coverage-percent` | Overall diff-coverage percentage |
| `report-path` | Path to the generated report file |

## Required Permissions

```yaml
permissions:
  pull-requests: write
  contents: read
```

## Consumer Workflow Example

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
        uses: your-org/Cocodif@v1
        with:
          coverage-files: '**/coverage.opencover.xml'
          fail-under: '80'
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Requirements

- `fetch-depth: 0` on `actions/checkout` for merge-base diff computation
- `.NET 10.0` SDK installed (the action sets this up automatically)
