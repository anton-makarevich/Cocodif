using System.Text.Json;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Reporters;

public class JsonReporter : IReporter
{
    public string Generate(DiffCoverageReport report)
    {
        var output = new
        {
            title = report.Title,
            summary = new
            {
                filesChecked = report.FilesChecked,
                filesWithGaps = report.FilesWithGaps,
                totalUncoveredLines = report.TotalUncoveredLines,
                totalChangedLines = report.TotalChangedLines,
                overallCoveragePercent = Math.Round(report.TotalCoveragePercent, 1)
            },
            files = report.Files
                .OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
                .Select(f => new
                {
                    path = f.RelativePath,
                    uncoveredCount = f.UncoveredChangedCount,
                    totalChangedLines = f.TotalChangedLines,
                    coveragePercent = Math.Round(f.CoveragePercent, 1),
                    uncoveredLines = f.UncoveredLines
                        .Where(l => f.ChangedLines.Count == 0 || f.ChangedLines.Contains(l))
                        .OrderBy(l => l)
                        .ToList()
                })
                .ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(output, options);
    }
}