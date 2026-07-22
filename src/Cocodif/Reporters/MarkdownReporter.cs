using System.Text;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Reporters;

public class MarkdownReporter : IReporter
{
    private readonly string _title;

    public MarkdownReporter(string title = "Coverage Report")
    {
        _title = title;
    }

    public string Generate(DiffCoverageReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## {_title}");
        sb.AppendLine();

        if (report.Files.Count == 0)
        {
            sb.AppendLine("No uncovered lines in changed files.");
            return sb.ToString();
        }

        sb.AppendLine("| File | Changed Lines | Uncovered Lines | Coverage |");
        sb.AppendLine("|------|---------------|-----------------|----------|");

        foreach (var file in report.Files.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var coverage = file.CoveragePercent;

            var relevantLines = file.UncoveredLines
                .Where(l => file.ChangedLines.Count == 0 || file.ChangedLines.Contains(l))
                .OrderBy(l => l)
                .ToList();

            var uncoveredDisplay = FormatLineRanges(relevantLines);

            sb.AppendLine($"| `{file.RelativePath}` | {file.TotalChangedLines} | {uncoveredDisplay} | {coverage:F1}% |");
        }

        sb.AppendLine();
        sb.AppendLine("### Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Files checked**: {report.FilesChecked}");
        sb.AppendLine($"- **Files with gaps**: {report.FilesWithGaps}");
        sb.AppendLine($"- **Total uncovered lines**: {report.TotalUncoveredLines}");
        sb.AppendLine($"- **Overall diff coverage**: {report.TotalCoveragePercent:F1}%");

        return sb.ToString();
    }

    internal static string FormatLineRanges(List<int> lines)
    {
        if (lines.Count == 0)
            return "-";

        var ranges = new List<string>();
        var rangeStart = lines[0];
        var rangeEnd = lines[0];

        for (var i = 1; i < lines.Count; i++)
        {
            if (lines[i] == rangeEnd + 1)
            {
                rangeEnd = lines[i];
            }
            else
            {
                ranges.Add(rangeStart == rangeEnd
                    ? rangeStart.ToString()
                    : $"{rangeStart}-{rangeEnd}");
                rangeStart = lines[i];
                rangeEnd = lines[i];
            }
        }

        ranges.Add(rangeStart == rangeEnd
            ? rangeStart.ToString()
            : $"{rangeStart}-{rangeEnd}");

        return string.Join(", ", ranges);
    }
}