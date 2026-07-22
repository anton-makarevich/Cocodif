using Sanet.Cocodif.Services;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Services;

public class DiffServiceTests
{
    [Fact]
    public void ComputeDiffCoverage_FiltersToChangedFilesOnly()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1, 2], [3]),
            ["src/B.cs"] = ([1], [2])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src/A.cs" };

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "**/*", "", "Test");

        report.Files.Count.ShouldBe(1);
        report.Files[0].RelativePath.ShouldBe("src/A.cs");
    }

    [Fact]
    public void ComputeDiffCoverage_AppliesIncludeGlob()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1], [2]),
            ["tests/B.cs"] = ([1], [2])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "src/A.cs", "tests/B.cs" };

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "src/**", "", "Test");

        report.Files.Count.ShouldBe(1);
        report.Files[0].RelativePath.ShouldBe("src/A.cs");
    }

    [Fact]
    public void ComputeDiffCoverage_AppliesExcludeGlob()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1], [2]),
            ["src/obj/B.cs"] = ([1], [2])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "src/A.cs", "src/obj/B.cs" };

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "**/*", "**/obj/**", "Test");

        report.Files.Count.ShouldBe(1);
        report.Files[0].RelativePath.ShouldBe("src/A.cs");
    }

    [Fact]
    public void ComputeDiffCoverage_EmptyChangedFiles_ReturnsEmptyReport()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1], [2])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "**/*", "", "Test");

        report.Files.ShouldBeEmpty();
        report.TotalCoveragePercent.ShouldBe(100.0);
    }

    [Fact]
    public void ComputeDiffCoverage_ComputesPerFileMetrics()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1, 2, 3, 4], [5, 6])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src/A.cs" };

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "**/*", "", "Test");

        report.Files.Count.ShouldBe(1);
        var file = report.Files[0];
        file.UncoveredChangedCount.ShouldBe(2);
        file.TotalChangedLines.ShouldBe(6);
        file.CoveragePercent.ShouldBe(66.7, 1);
    }

    [Fact]
    public void ComputeDiffCoverage_OverallMetrics()
    {
        var merged = new Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)>
        {
            ["src/A.cs"] = ([1, 2, 3, 4], [5]),
            ["src/B.cs"] = ([1, 2], [3, 4, 5])
        };
        var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "src/A.cs", "src/B.cs" };

        var report = DiffService.ComputeDiffCoverage(
            merged, changedFiles, "**/*", "", "Test");

        report.FilesChecked.ShouldBe(2);
        report.FilesWithGaps.ShouldBe(2);
        report.TotalUncoveredLines.ShouldBe(4);
        report.TotalChangedLines.ShouldBe(10);
        report.TotalCoveragePercent.ShouldBe(60.0, 1);
    }
}
