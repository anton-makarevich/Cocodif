namespace Sanet.Cocodif.Models;

public class CoverageData
{
    public Dictionary<string, string> Files { get; set; } = new();
    public List<UncoveredLine> UncoveredLines { get; set; } = [];
}