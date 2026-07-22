using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Services;

public static class DiffService
{
    public static DiffCoverageReport ComputeDiffCoverage(
        Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)> mergedCoverage,
        HashSet<string> changedFiles,
        string includePattern,
        string excludePattern,
        string title)
    {
        var report = new DiffCoverageReport { Title = title };

        foreach (var (relativePath, (covered, uncovered)) in mergedCoverage)
        {
            if (!changedFiles.Contains(relativePath))
                continue;

            if (!GlobMatcher.Matches(relativePath, includePattern))
                continue;

            if (GlobMatcher.IsExcluded(relativePath, excludePattern))
                continue;

            var allLines = new SortedSet<int>(covered);
            allLines.UnionWith(uncovered);

            var info = new FileCoverageInfo
            {
                RelativePath = relativePath,
                CoveredLines = covered,
                UncoveredLines = uncovered
            };

            report.Files.Add(info);
        }

        return report;
    }
}
