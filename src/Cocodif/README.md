# Cocodif

Diff-coverage reporter for .NET. Parses OpenCover or Cobertura XML coverage reports, computes coverage only for changed files, and emits Markdown and/or JSON reports.

## Install

```bash
dotnet tool install --global Sanet.Cocodif
```

## Usage

```bash
Cocodif -c coverage.xml -f changed-files.txt -o report.md
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--coverage` | `-c` | One or more paths to coverage XML files (OpenCover or Cobertura). Multiple inputs are merged. |
| `--changed-files` | `-f` | Path to a file containing changed file paths, one per line. |
| `--format` | | `opencover`, `cobertura`, or `auto` (default: `auto`). Auto-detects from XML root element. |
| `--root` | | Repository root for normalizing absolute paths to repo-relative. |
| `--include` | | Glob pattern for files to include (default: `**/*`). |
| `--exclude` | | Glob pattern for files to exclude (default: `**/obj/**,**/bin/**`). |
| `--title` | | Report title (default: `Coverage Report`). |
| `--output-format` | | `markdown`, `json`, or `both` (default: `markdown`). |
| `--output` | `-o` | Output file path(s). If omitted, writes to stdout. |
| `--fail-under` | | Fail with exit code 1 if overall diff-coverage % is below this threshold. |

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error, or coverage below `--fail-under` threshold |
| 2 | Invalid argument |
| 3 | File not found |

### Examples

**Multi-module merge:**
```bash
Cocodif -c core.xml -c api.xml -f changed.txt --output both --output-format both
```

**JSON output for CI scripting:**
```bash
Cocodif -c coverage.xml -f changed.txt --output-format json -o report.json
```

**Fail build on low coverage:**
```bash
Cocodif -c coverage.xml -f changed.txt --fail-under 80
```

### Output Formats

**Markdown** — per-file table with uncovered line numbers, coverage %, and a summary section.

**JSON** — structured data with per-file entries and overall totals, suitable for programmatic consumption.

## License

MIT
