using System.Xml.Linq;
using Sanet.Cocodif.Parsers;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Parsers;

public class ParserFactoryTests
{
    [Fact]
    public void DetectFormat_CoverageSession_ReturnsOpenCover()
    {
        var doc = XDocument.Parse("<CoverageSession><Modules/></CoverageSession>");
        ParserFactory.DetectFormat(doc).ShouldBe(CoverageFormat.OpenCover);
    }

    [Fact]
    public void DetectFormat_Coverage_ReturnsCobertura()
    {
        var doc = XDocument.Parse("<coverage><packages/></coverage>");
        ParserFactory.DetectFormat(doc).ShouldBe(CoverageFormat.Cobertura);
    }

    [Fact]
    public void DetectFormat_UnknownElement_Throws()
    {
        var doc = XDocument.Parse("<unknown/>");
        Should.Throw<NotSupportedException>(() => ParserFactory.DetectFormat(doc));
    }

    [Fact]
    public void Create_Auto_OpenCover_DetectsAndParses()
    {
        var assembly = typeof(ParserFactoryTests).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith("opencover-sample.xml", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        var doc = XDocument.Load(stream);

        var parser = ParserFactory.Create(CoverageFormat.Auto, doc);
        parser.ShouldBeOfType<OpenCoverParser>();

        var result = parser.Parse(doc);
        result.UncoveredLines.Count.ShouldBe(4);
    }

    [Fact]
    public void Create_Auto_Cobertura_DetectsAndParses()
    {
        var assembly = typeof(ParserFactoryTests).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith("cobertura-sample.xml", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        var doc = XDocument.Load(stream);

        var parser = ParserFactory.Create(CoverageFormat.Auto, doc);
        parser.ShouldBeOfType<CoberturaParser>();

        var result = parser.Parse(doc);
        result.UncoveredLines.Count.ShouldBe(4);
    }

    [Fact]
    public void Create_ExplicitFormat_ReturnsCorrectParser()
    {
        var doc = XDocument.Parse("<anything/>");

        ParserFactory.Create(CoverageFormat.OpenCover, doc)
            .ShouldBeOfType<OpenCoverParser>();

        ParserFactory.Create(CoverageFormat.Cobertura, doc)
            .ShouldBeOfType<CoberturaParser>();
    }
}
