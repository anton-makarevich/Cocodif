using System.Text.Json;
using Sanet.Cocodif.Models;
using Sanet.Cocodif.Reporters;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Reporters;

public class JsonReporterTests
{
    [Fact]
    public void Generate_ValidReport_ProducesValidJson()
    {
        var report = new DiffCoverageReport
        {
            Title = "Test",
            Files =
            [
                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [1, 2, 3, 4],
                    UncoveredLines = [5]
                }
            ]
        };
        var reporter = new JsonReporter();

        var result = reporter.Generate(report);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.ShouldNotBe(default);
    }

    [Fact]
    public void Generate_ContainsExpectedStructure()
    {
        var report = new DiffCoverageReport
        {
            Title = "Coverage",
            Files =
            [
                new()
                {
                    RelativePath = "src/A.cs",
                    CoveredLines = [1, 2, 3],
                    UncoveredLines = [4]
                }
            ]
        };
        var reporter = new JsonReporter();

        var result = reporter.Generate(report);
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("title").GetString().ShouldBe("Coverage");
        root.GetProperty("summary").GetProperty("filesChecked").GetInt32().ShouldBe(1);
        root.GetProperty("summary").GetProperty("overallCoveragePercent").GetDouble().ShouldBe(75.0, 1);
        root.GetProperty("files").GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public void Generate_EmptyReport_HasZeroSummary()
    {
        var report = new DiffCoverageReport { Title = "Empty" };
        var reporter = new JsonReporter();

        var result = reporter.Generate(report);
        var doc = JsonDocument.Parse(result);
        var summary = doc.RootElement.GetProperty("summary");

        summary.GetProperty("filesChecked").GetInt32().ShouldBe(0);
        summary.GetProperty("totalUncoveredLines").GetInt32().ShouldBe(0);
        summary.GetProperty("overallCoveragePercent").GetDouble().ShouldBe(100.0);
    }
}
