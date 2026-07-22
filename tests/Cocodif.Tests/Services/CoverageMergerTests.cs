using Sanet.Cocodif.Models;
using Sanet.Cocodif.Services;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Services;

public class CoverageMergerTests
{
    [Fact]
    public void Add_SingleReport_CorrectlyMerges()
    {
        var normalizer = new PathNormalizer("/home/user/repo");
        var merger = new CoverageMerger();

        var data = new CoverageData
        {
            Files = new Dictionary<string, string>
            {
                ["1"] = "/home/user/repo/src/Program.cs"
            },
            UncoveredLines =
            [
                new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 10 },
                new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 20 }
            ]
        };

        merger.Add(data, normalizer);
        var merged = merger.GetMerged();

        merged.Count.ShouldBe(1);
        merged["src/Program.cs"].Uncovered.ShouldBe([10, 20]);
    }

    [Fact]
    public void Add_MultipleReports_MergesCorrectly()
    {
        var normalizer = new PathNormalizer("/home/user/repo");
        var merger = new CoverageMerger();

        var data1 = new CoverageData
        {
            Files = new Dictionary<string, string>
            {
                ["1"] = "/home/user/repo/src/Program.cs"
            },
            UncoveredLines = [new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 10 }]
        };

        var data2 = new CoverageData
        {
            Files = new Dictionary<string, string>
            {
                ["1"] = "/home/user/repo/src/Program.cs"
            },
            UncoveredLines = [new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 20 }]
        };

        merger.Add(data1, normalizer);
        merger.Add(data2, normalizer);
        var merged = merger.GetMerged();

        merged["src/Program.cs"].Uncovered.ShouldBe([10, 20]);
    }

    [Fact]
    public void Add_DuplicateUncoveredLines_Deduplicates()
    {
        var normalizer = new PathNormalizer("/home/user/repo");
        var merger = new CoverageMerger();

        var data1 = new CoverageData
        {
            Files = new Dictionary<string, string>
            {
                ["1"] = "/home/user/repo/src/Program.cs"
            },
            UncoveredLines = [new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 10 }]
        };

        var data2 = new CoverageData
        {
            Files = new Dictionary<string, string>
            {
                ["1"] = "/home/user/repo/src/Program.cs"
            },
            UncoveredLines = [new UncoveredLine { FilePath = "/home/user/repo/src/Program.cs", LineNumber = 10 }]
        };

        merger.Add(data1, normalizer);
        merger.Add(data2, normalizer);
        var merged = merger.GetMerged();

        merged["src/Program.cs"].Uncovered.Count.ShouldBe(1);
    }
}
