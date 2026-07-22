using System.Xml.Linq;
using Sanet.Cocodif.Parsers;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Parsers;

public class CoberturaParserTests
{
    private readonly CoberturaParser _parser = new();

    [Fact]
    public void Parse_ValidCobertura_ExtractsUncoveredLines()
    {
        var doc = LoadFixture("cobertura-sample.xml");
        var result = _parser.Parse(doc);

        result.UncoveredLines.Count.ShouldBe(4);

        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "src/App/Program.cs" && ul.LineNumber == 12);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "src/App/Program.cs" && ul.LineNumber == 20);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "src/App/Services/AuthService.cs" && ul.LineNumber == 5);
        result.UncoveredLines.ShouldContain(ul =>
            ul.FilePath == "src/App/Services/AuthService.cs" && ul.LineNumber == 10);
    }

    [Fact]
    public void Parse_SkipsLinesWithHits()
    {
        var doc = LoadFixture("cobertura-sample.xml");
        var result = _parser.Parse(doc);

        result.UncoveredLines.ShouldNotContain(ul =>
            ul.FilePath == "src/App/Program.cs" && ul.LineNumber == 10);
        result.UncoveredLines.ShouldNotContain(ul =>
            ul.FilePath == "src/App/Models/User.cs" && ul.LineNumber == 3);
    }

    [Fact]
    public void Parse_Cobertura_AllLinesCovered()
    {
        var doc = LoadFixture("cobertura-sample.xml");
        var result = _parser.Parse(doc);

        var userLines = result.UncoveredLines
            .Where(ul => ul.FilePath == "src/App/Models/User.cs")
            .ToList();

        userLines.ShouldBeEmpty();
    }

    private static XDocument LoadFixture(string name)
    {
        var assembly = typeof(CoberturaParserTests).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return XDocument.Load(stream);
    }
}
