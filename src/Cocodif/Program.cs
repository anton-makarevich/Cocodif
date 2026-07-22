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
        var coverageOption = new Option<FileInfo[]>(
            name: "--coverage",
            description: "Path(s) to coverage XML file(s) (OpenCover or Cobertura)")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        coverageOption.AddAlias("-c");

        var formatOption = new Option<string>(
            name: "--format",
            description: "Coverage format: opencover, cobertura, or auto",
            getDefaultValue: () => "auto");

        var changedFilesOption = new Option<FileInfo>(
            name: "--changed-files",
            description: "Path to file containing list of changed files (one per line)")
        {
            IsRequired = true
        };
        changedFilesOption.AddAlias("-f");

        var rootOption = new Option<DirectoryInfo?>(
            name: "--root",
            description: "Repository root for normalizing absolute paths to repo-relative");

        var includeOption = new Option<string>(
            name: "--include",
            description: "Glob pattern for files to include",
            getDefaultValue: () => "**/*");

        var excludeOption = new Option<string>(
            name: "--exclude",
            description: "Glob pattern for files to exclude",
            getDefaultValue: () => "**/obj/**,**/bin/**");

        var titleOption = new Option<string>(
            name: "--title",
            description: "Report title",
            getDefaultValue: () => "Coverage Report");

        var outputFormatOption = new Option<string>(
            name: "--output-format",
            description: "Output format: markdown, json, or both",
            getDefaultValue: () => "markdown");

        var outputOption = new Option<FileInfo?>(
            name: "--output",
            description: "Output file path(s)");
        outputOption.AddAlias("-o");

        var failUnderOption = new Option<double?>(
            name: "--fail-under",
            description: "Fail (exit code 1) if overall diff coverage is below this percentage");

        var rootCommand = new RootCommand(
            "Diff-coverage reporter — parses coverage XML and reports coverage on changed files")
        {
            coverageOption, formatOption, changedFilesOption, rootOption,
            includeOption, excludeOption, titleOption, outputFormatOption,
            outputOption, failUnderOption
        };

        rootCommand.SetHandler(async (context) =>
        {
            var coverageFiles = context.ParseResult.GetValueForOption(coverageOption)!;
            var format = context.ParseResult.GetValueForOption(formatOption)!;
            var changedFilesPath = context.ParseResult.GetValueForOption(changedFilesOption)!;
            var root = context.ParseResult.GetValueForOption(rootOption);
            var include = context.ParseResult.GetValueForOption(includeOption)!;
            var exclude = context.ParseResult.GetValueForOption(excludeOption)!;
            var title = context.ParseResult.GetValueForOption(titleOption)!;
            var outputFormat = context.ParseResult.GetValueForOption(outputFormatOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption);
            var failUnder = context.ParseResult.GetValueForOption(failUnderOption);

            context.ExitCode = await RunAsync(
                coverageFiles, format, changedFilesPath, root,
                include, exclude, title, outputFormat, output, failUnder);
        });

        return await rootCommand.InvokeAsync(args);
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
                    Console.Error.WriteLine($"Error: Coverage file not found: {file.FullName}");
                    return 3;
                }

                var doc = XDocument.Load(file.FullName);
                var parser = ParserFactory.Create(coverageFormat, doc);
                var data = parser.Parse(doc);
                merger.Add(data, normalizer);
            }

            if (!changedFilesPath.Exists)
            {
                Console.Error.WriteLine($"Error: Changed files list not found: {changedFilesPath.FullName}");
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
                Console.Error.WriteLine("Error: Invalid output format. Use markdown, json, or both.");
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
                    Console.Error.WriteLine($"Report written to: {outputPath}");
                }
                else
                {
                    Console.Write(content);
                }
            }

            if (failUnder.HasValue && report.TotalCoveragePercent < failUnder.Value)
            {
                Console.Error.WriteLine(
                    $"Coverage {report.TotalCoveragePercent:F1}% is below threshold {failUnder.Value:F1}%");
                return 1;
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
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
