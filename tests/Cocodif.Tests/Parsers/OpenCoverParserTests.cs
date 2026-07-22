using System.Xml.Linq;
using Sanet.Cocodif.Parsers;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Parsers;

public class OpenCoverParserTests
{
    private readonly OpenCoverParser _parser = new();

    [Fact]
    public void Parse_ValidOpenCover_ExtractsFileMapping()
    {
        var doc = LoadFixture("opencover-sample.xml");
        var result = _parser.Parse(doc);

        result.Files.Count.ShouldBe(3);
        result.Files["1"].ShouldBe("/home/user/repo/src/App/Program.cs");
        result.Files["2"].ShouldBe("/home/user/repo/src/App/Services/AuthService.cs");
        result.Files["3"].ShouldBe("/home/user/repo/src/App/Models/User.cs");
    }

    [Fact]
    public void Parse_ValidOpenCover_ExtractsUncoveredLines()
    {
        var doc = LoadFixture("opencover-sample.xml");
        var result = _parser.Parse(doc);

        result.UncoveredLines.Count.ShouldBe(4);

        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Program.cs" && ul.LineNumber == 12);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Program.cs" && ul.LineNumber == 20);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Services/AuthService.cs" && ul.LineNumber == 5);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Services/AuthService.cs" && ul.LineNumber == 10);
    }

    [Fact]
    public void Parse_SkipsCoveredLines()
    {
        var doc = LoadFixture("opencover-sample.xml");
        var result = _parser.Parse(doc);

        result.UncoveredLines.ShouldNotContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Program.cs" && ul.LineNumber == 10);
        result.UncoveredLines.ShouldNotContain(ul =>
            ul.FilePath == "/home/user/repo/src/App/Models/User.cs" && ul.LineNumber == 3);
    }

    [Fact]
    public void Parse_UserModel_AllLinesCovered()
    {
        var doc = LoadFixture("opencover-sample.xml");
        var result = _parser.Parse(doc);

        var userLines = result.UncoveredLines
            .Where(ul => ul.FilePath == "/home/user/repo/src/App/Models/User.cs")
            .ToList();

        userLines.ShouldBeEmpty();
    }

    private static XDocument LoadFixture(string name)
    {
        var assembly = typeof(OpenCoverParserTests).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return XDocument.Load(stream);
    }
}
