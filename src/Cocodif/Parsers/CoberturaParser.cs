using System.Xml.Linq;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Parsers;

public class CoberturaParser : ICoverageParser
{
    public CoverageData Parse(XDocument document)
    {
        var files = new Dictionary<string, string>();
        var uncoveredLines = new List<UncoveredLine>();

        var packages = document.Root?.Elements("packages").Elements("package")
                    ?? document.Root?.Elements("package")
                    ?? [];

        var fileIndex = 1;

        foreach (var package in packages)
        {
            var classes = package.Element("classes")?.Elements("class") ?? [];
            foreach (var cls in classes)
            {
                var filename = cls.Attribute("filename")?.Value;
                if (filename == null) continue;

                if (!files.ContainsValue(filename))
                {
                    files[fileIndex.ToString()] = filename;
                    fileIndex++;
                }

                var lines = cls.Element("lines")?.Elements("line") ?? [];
                foreach (var line in lines)
                {
                    var hits = line.Attribute("hits")?.Value;
                    var lineNumber = line.Attribute("number")?.Value;

                    if (hits == "0" && lineNumber != null)
                    {
                        uncoveredLines.Add(new UncoveredLine
                        {
                            FilePath = filename,
                            LineNumber = int.Parse(lineNumber)
                        });
                    }
                }
            }
        }

        return new CoverageData
        {
            Files = files,
            UncoveredLines = uncoveredLines
        };
    }
}
