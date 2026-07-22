namespace Sanet.Cocodif.Models;

public class DiffCoverageReport
{
    public string Title { get; set; } = "Coverage Report";
    public List<FileCoverageInfo> Files { get; set; } = [];
    public int TotalUncoveredLines => Files.Sum(f => f.UncoveredChangedCount);
    public int TotalChangedLines => Files.Sum(f => f.TotalChangedLines);
    public double TotalCoveragePercent => TotalChangedLines > 0
        ? (1.0 - (double)TotalUncoveredLines / TotalChangedLines) * 100.0
        : 100.0;
    public int FilesChecked => Files.Count;
    public int FilesWithGaps => Files.Count(f => f.UncoveredChangedCount > 0);
}