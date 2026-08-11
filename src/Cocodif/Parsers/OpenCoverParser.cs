using System.Xml.Linq;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Parsers;

public class OpenCoverParser : ICoverageParser
{
    public CoverageData Parse(XDocument document)
    {
        var files = new Dictionary<string, string>();
        var uncoveredLines = new List<SourceLine>();
        var coveredLines = new List<SourceLine>();

        var modules = document.Root?.Element("Modules")?.Elements("Module") ?? [];

        var xElements = modules as XElement[] ?? modules.ToArray();
        foreach (var module in xElements)
        {
            var fileElements = module.Element("Files")?.Elements("File") ?? [];
            foreach (var fe in fileElements)
            {
                var uid = fe.Attribute("uid")?.Value;
                var fullPath = fe.Attribute("fullPath")?.Value;
                if (uid != null && fullPath != null)
                    files[uid] = fullPath;
            }
        }

        var sequencePoints = xElements
            .Elements("Classes").Elements("Class")
            .Elements("Methods").Elements("Method")
            .Elements("SequencePoints").Elements("SequencePoint");

        foreach (var sp in sequencePoints)
        {
            var vc = sp.Attribute("vc")?.Value;
            var fileId = sp.Attribute("fileid")?.Value;
            var sl = sp.Attribute("sl")?.Value;

            if (fileId == null || sl == null || !files.TryGetValue(fileId, out var filePath))
                continue;

            if (vc == "0")
            {
                uncoveredLines.Add(new SourceLine
                {
                    FilePath = filePath,
                    LineNumber = int.Parse(sl)
                });
            }
            else if (vc != null)
            {
                coveredLines.Add(new SourceLine
                {
                    FilePath = filePath,
                    LineNumber = int.Parse(sl)
                });
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
