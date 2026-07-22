using System.Xml.Linq;
using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Parsers;

public interface ICoverageParser
{
    CoverageData Parse(XDocument document);
}