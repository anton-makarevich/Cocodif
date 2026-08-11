namespace Sanet.Cocodif.Models;

public class CoverageData
{
    public Dictionary<string, string> Files { get; set; } = new();
    public List<SourceLine> UncoveredLines { get; set; } = [];
    public List<SourceLine> CoveredLines { get; set; } = [];
}