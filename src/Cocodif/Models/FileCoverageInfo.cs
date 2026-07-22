namespace Sanet.Cocodif.Models;

public class FileCoverageInfo
{
    public required string RelativePath { get; init; }
    public SortedSet<int> CoveredLines { get; set; } = [];
    public SortedSet<int> UncoveredLines { get; set; } = [];
    public SortedSet<int> ChangedLines { get; set; } = [];

    public int UncoveredChangedCount => ChangedLines.Count > 0
        ? UncoveredLines.Intersect(ChangedLines).Count()
        : UncoveredLines.Count;

    public int TotalChangedLines => ChangedLines.Count > 0
        ? ChangedLines.Count
        : CoveredLines.Count + UncoveredLines.Count;

    public double CoveragePercent => TotalChangedLines > 0
        ? (1.0 - (double)UncoveredChangedCount / TotalChangedLines) * 100.0
        : 100.0;
}