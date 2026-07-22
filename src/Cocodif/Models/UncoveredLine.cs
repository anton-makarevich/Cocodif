namespace Sanet.Cocodif.Models;

public class UncoveredLine
{
    public required string FilePath { get; init; }
    public required int LineNumber { get; init; }
}