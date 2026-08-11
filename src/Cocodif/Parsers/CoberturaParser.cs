using System.Xml.Linq;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Parsers;

public class CoberturaParser : ICoverageParser
{
    public CoverageData Parse(XDocument document)
    {
        var files = new Dictionary<string, string>();
        var uncoveredLines = new List<SourceLine>();
        var coveredLines = new List<SourceLine>();

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

                    if (lineNumber == null)
                        continue;

                    if (hits == "0")
                    {
                        uncoveredLines.Add(new SourceLine
                        {
                            FilePath = filename,
                            LineNumber = int.Parse(lineNumber)
                        });
                    }
                    else if (hits != null)
                    {
                        coveredLines.Add(new SourceLine
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
            UncoveredLines = uncoveredLines,
            CoveredLines = coveredLines
        };
    }
}
