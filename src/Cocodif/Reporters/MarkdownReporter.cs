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

        sb.AppendLine("| File | Uncovered Lines | Changed Lines | Coverage |");
        sb.AppendLine("|------|----------------|---------------|----------|");

        foreach (var file in report.Files.OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var uncoveredCount = file.UncoveredChangedCount;
            var coverage = file.CoveragePercent;

            var lineNumbers = file.UncoveredLines
                .Where(l => file.ChangedLines.Count == 0 || file.ChangedLines.Contains(l))
                .OrderBy(l => l)
                .Take(20)
                .Select(l => l.ToString());

            var uncoveredDisplay = uncoveredCount > 0
                ? string.Join(", ", lineNumbers) + (uncoveredCount > 20 ? $" (+{uncoveredCount - 20} more)" : "")
                : "-";

            sb.AppendLine($"| `{file.RelativePath}` | {uncoveredCount} | {file.TotalChangedLines} | {coverage:F1}% |");
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
}