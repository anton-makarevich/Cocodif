using Sanet.Cocodif.Models;
using Sanet.Cocodif.Reporters;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Reporters;

public class MarkdownReporterTests
{
    [Fact]
    public void Generate_EmptyReport_ShowsNoUncoveredMessage()
    {
        var report = new DiffCoverageReport { Title = "Test Report" };
        var reporter = new MarkdownReporter("Test Report");

        var result = reporter.Generate(report);

        result.ShouldContain("## Test Report");
        result.ShouldContain("No uncovered lines in changed files");
    }

    [Fact]
    public void Generate_WithFiles_ShowsTableAndSummary()
    {
        var report = new DiffCoverageReport
        {
            Title = "PR Coverage",
            Files =
            [
                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [1, 2, 3, 4],
                    UncoveredLines = [5, 6]
                }
            ]
        };
        var reporter = new MarkdownReporter("PR Coverage");

        var result = reporter.Generate(report);

        result.ShouldContain("## PR Coverage");
        result.ShouldContain("| File |");
        result.ShouldContain("src/A.cs");
        result.ShouldContain("### Summary");
        result.ShouldContain("Files checked**: 1");
        result.ShouldContain("Overall diff coverage**: 66.7%");
    }

    [Fact]
    public void Generate_MultipleFiles_OrderByPath()
    {
        var report = new DiffCoverageReport
        {
            Title = "Test",
            Files =
            [
                new()
                {
                    RelativePath = "src/Z.cs",
                    CoveredLines = [1],
                    UncoveredLines = []
                },

                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [1],
                    UncoveredLines = []
                }
            ]
        };
        var reporter = new MarkdownReporter();

        var result = reporter.Generate(report);
        var lines = result.Split('\n');

        var aLine = Array.FindIndex(lines, l => l.Contains("src/A.cs"));
        var zLine = Array.FindIndex(lines, l => l.Contains("src/Z.cs"));

        aLine.ShouldBeLessThan(zLine);
    }
}
