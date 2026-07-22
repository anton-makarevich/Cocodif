using System.Xml.Linq;

namespace Sanet.Cocodif.Parsers;

public static class ParserFactory
{
    public static ICoverageParser Create(CoverageFormat format, XDocument document)
    {
        if (format == CoverageFormat.Auto)
            format = DetectFormat(document);

        return format switch
        {
            CoverageFormat.OpenCover => new OpenCoverParser(),
            CoverageFormat.Cobertura => new CoberturaParser(),
            _ => throw new NotSupportedException($"Unsupported coverage format: {format}")
        };
    }

    public static CoverageFormat DetectFormat(XDocument document)
    {
        var rootName = document.Root?.Name.LocalName;
        return rootName switch
        {
            "CoverageSession" => CoverageFormat.OpenCover,
            "coverage" => CoverageFormat.Cobertura,
            _ => throw new NotSupportedException(
                $"Unable to auto-detect coverage format from root element '{rootName}'. " +
                "Use --format to specify opencover or cobertura explicitly.")
        };
    }
}
