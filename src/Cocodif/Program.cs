using System.CommandLine;
using System.Xml.Linq;
using Sanet.Cocodif.Parsers;
using Sanet.Cocodif.Reporters;
using Sanet.Cocodif.Services;

namespace Sanet.Cocodif;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var coverageOption = new Option<FileInfo[]>("--coverage", "-c")
        {
            Description = "Path(s) to coverage XML file(s) (OpenCover or Cobertura)"
        };

        var formatOption = new Option<string>("--format")
        {
            Description = "Coverage format: opencover, cobertura, or auto",
            DefaultValueFactory = _ => "auto"
        };

        var changedFilesOption = new Option<FileInfo>("--changed-files", "-f")
        {
            Description = "Path to file containing list of changed files (one per line)"
        };

        var rootOption = new Option<DirectoryInfo?>("--root")
        {
            Description = "Repository root for normalizing absolute paths to repo-relative"
        };

        var includeOption = new Option<string>("--include")
        {
            Description = "Glob pattern for files to include",
            DefaultValueFactory = _ => "**/*"
        };

        var excludeOption = new Option<string>("--exclude")
        {
            Description = "Glob pattern for files to exclude",
            DefaultValueFactory = _ => "**/obj/**,**/bin/**"
        };

        var titleOption = new Option<string>("--title")
        {
            Description = "Report title",
            DefaultValueFactory = _ => "Coverage Report"
        };

        var outputFormatOption = new Option<string>("--output-format")
        {
            Description = "Output format: markdown, json, or both",
            DefaultValueFactory = _ => "markdown"
        };

        var outputOption = new Option<FileInfo?>("--output", "-o")
        {
            Description = "Output file path(s)"
        };

        var failUnderOption = new Option<double?>("--fail-under")
        {
            Description = "Fail (exit code 1) if overall diff coverage is below this percentage"
        };

        var rootCommand = new RootCommand(
            "Diff-coverage reporter — parses coverage XML and reports coverage on changed files")
        {
            coverageOption, formatOption, changedFilesOption, rootOption,
            includeOption, excludeOption, titleOption, outputFormatOption,
            outputOption, failUnderOption
        };

        rootCommand.SetAction(async (parseResult, _) =>
        {
            var coverageFiles = parseResult.GetValue(coverageOption)!;
            var format = parseResult.GetValue(formatOption)!;
            var changedFilesPath = parseResult.GetValue(changedFilesOption)!;
            var root = parseResult.GetValue(rootOption);
            var include = parseResult.GetValue(includeOption)!;
            var exclude = parseResult.GetValue(excludeOption)!;
            var title = parseResult.GetValue(titleOption)!;
            var outputFormat = parseResult.GetValue(outputFormatOption)!;
            var output = parseResult.GetValue(outputOption);
            var failUnder = parseResult.GetValue(failUnderOption);

            return await RunAsync(
                coverageFiles, format, changedFilesPath, root,
                include, exclude, title, outputFormat, output, failUnder);
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    internal static async Task<int> RunAsync(
        FileInfo[] coverageFiles,
        string format,
        FileInfo changedFilesPath,
        DirectoryInfo? root,
        string include,
        string exclude,
        string title,
        string outputFormat,
        FileInfo? output,
        double? failUnder)
    {
        try
        {
            var coverageFormat = ParseFormat(format);
            var repoRoot = root?.FullName ?? Directory.GetCurrentDirectory();
            var normalizer = new PathNormalizer(repoRoot);
            var merger = new CoverageMerger();

            foreach (var file in coverageFiles)
            {
                if (!file.Exists)
                {
                    await Console.Error.WriteLineAsync($"Error: Coverage file not found: {file.FullName}");
                    return 3;
                }

                var doc = XDocument.Load(file.FullName);
                var parser = ParserFactory.Create(coverageFormat, doc);
                var data = parser.Parse(doc);
                merger.Add(data, normalizer);
            }

            if (!changedFilesPath.Exists)
            {
                await Console.Error.WriteLineAsync($"Error: Changed files list not found: {changedFilesPath.FullName}");
                return 3;
            }

            var changedLines = await File.ReadAllLinesAsync(changedFilesPath.FullName);
            var changedFiles = new HashSet<string>(
                changedLines
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var merged = merger.GetMerged();

            var report = DiffService.ComputeDiffCoverage(
                merged, changedFiles, include, exclude, title);

            var reporters = new List<IReporter>();
            if (outputFormat.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                outputFormat.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                reporters.Add(new JsonReporter());
            }
            if (outputFormat.Equals("markdown", StringComparison.OrdinalIgnoreCase) ||
                outputFormat.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                reporters.Add(new MarkdownReporter(title));
            }

            if (reporters.Count == 0)
            {
                await Console.Error.WriteLineAsync("Error: Invalid output format. Use markdown, json, or both.");
                return 2;
            }

            foreach (var reporter in reporters)
            {
                var content = reporter.Generate(report);

                if (output != null)
                {
                    var ext = reporter is JsonReporter ? ".json" : ".md";
                    var outputPath = reporters.Count > 1
                        ? Path.ChangeExtension(output.FullName, ext)
                        : output.FullName;

                    await File.WriteAllTextAsync(outputPath, content);
                    await Console.Error.WriteLineAsync($"Report written to: {outputPath}");
                }
                else
                {
                    Console.Write(content);
                }
            }

            if (failUnder.HasValue && report.TotalCoveragePercent < failUnder.Value)
            {
                await Console.Error.WriteLineAsync(
                    $"Coverage {report.TotalCoveragePercent:F1}% is below threshold {failUnder.Value:F1}%");
                return 1;
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 2;
        }
    }

    private static CoverageFormat ParseFormat(string format) =>
        format.ToLowerInvariant() switch
        {
            "opencover" => CoverageFormat.OpenCover,
            "cobertura" => CoverageFormat.Cobertura,
            "auto" => CoverageFormat.Auto,
            _ => throw new ArgumentException($"Invalid format '{format}'. Use opencover, cobertura, or auto.")
        };
}
