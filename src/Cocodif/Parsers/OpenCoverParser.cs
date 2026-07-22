using System.Xml.Linq;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Parsers;

public class OpenCoverParser : ICoverageParser
{
    public CoverageData Parse(XDocument document)
    {
        var files = new Dictionary<string, string>();
        var uncoveredLines = new List<UncoveredLine>();

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
            if (sp.Attribute("vc")?.Value != "0") continue;

            var fileId = sp.Attribute("fileid")?.Value;
            var sl = sp.Attribute("sl")?.Value;

            if (fileId != null && sl != null && files.TryGetValue(fileId, out var filePath))
            {
                uncoveredLines.Add(new UncoveredLine
                {
                    FilePath = filePath,
                    LineNumber = int.Parse(sl)
                });
            }
        }

        return new CoverageData
        {
            Files = files,
            UncoveredLines = uncoveredLines
        };
    }
}
