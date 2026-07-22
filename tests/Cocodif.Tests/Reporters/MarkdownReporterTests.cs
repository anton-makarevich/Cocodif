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
        result.ShouldContain("| Changed Lines |");
        result.ShouldContain("| Uncovered Lines |");
        result.ShouldContain("src/A.cs");
        result.ShouldContain("5-6");
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

    [Fact]
    public void Generate_ShowsRangeFormattedUncoveredLines()
    {
        var report = new DiffCoverageReport
        {
            Title = "Test",
            Files =
            [
                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [10],
                    UncoveredLines = [1, 2, 3, 5, 6, 10, 11, 12, 13]
                }
            ]
        };
        var reporter = new MarkdownReporter();

        var result = reporter.Generate(report);

        result.ShouldContain("1-3, 5-6, 10-13");
    }

    [Fact]
    public void Generate_NoUncoveredLines_ShowsDash()
    {
        var report = new DiffCoverageReport
        {
            Title = "Test",
            Files =
            [
                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [1, 2, 3],
                    UncoveredLines = []
                }
            ]
        };
        var reporter = new MarkdownReporter();

        var result = reporter.Generate(report);

        result.ShouldContain("| `src/A.cs` | 3 | - | 100.0% |");
    }

    [Fact]
    public void FormatLineRanges_EmptyList_ReturnsDash()
    {
        MarkdownReporter.FormatLineRanges([]).ShouldBe("-");
    }

    [Fact]
    public void FormatLineRanges_SingleLine_ReturnsLineNumber()
    {
        MarkdownReporter.FormatLineRanges([42]).ShouldBe("42");
    }

    [Fact]
    public void FormatLineRanges_ConsecutiveLines_ReturnsRange()
    {
        MarkdownReporter.FormatLineRanges([1, 2, 3]).ShouldBe("1-3");
    }

    [Fact]
    public void FormatLineRanges_MixedLines_ReturnsCorrectRanges()
    {
        MarkdownReporter.FormatLineRanges([1, 2, 3, 7, 10, 11, 12])
            .ShouldBe("1-3, 7, 10-12");
    }

    [Fact]
    public void FormatLineRanges_AllConsecutive_ReturnsSingleRange()
    {
        MarkdownReporter.FormatLineRanges([5, 6, 7, 8, 9]).ShouldBe("5-9");
    }

    [Fact]
    public void FormatLineRanges_NoConsecutive_ReturnsCommaSeparated()
    {
        MarkdownReporter.FormatLineRanges([1, 5, 10]).ShouldBe("1, 5, 10");
    }
}
