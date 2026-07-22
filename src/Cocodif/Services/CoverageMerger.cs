using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Services;

public class CoverageMerger
{
    private readonly Dictionary<string, SortedSet<int>> _coveredByFile = new();
    private readonly Dictionary<string, SortedSet<int>> _uncoveredByFile = new();

    public void Add(CoverageData data, PathNormalizer normalizer)
    {
        var grouped = data.UncoveredLines
            .GroupBy(ul => normalizer.Normalize(ul.FilePath));

        foreach (var group in grouped)
        {
            var relativePath = group.Key;

            if (!_uncoveredByFile.TryGetValue(relativePath, out var uncovered))
            {
                uncovered = [];
                _uncoveredByFile[relativePath] = uncovered;
            }

            foreach (var line in group)
                uncovered.Add(line.LineNumber);

            if (!_coveredByFile.TryGetValue(relativePath, out var covered))
            {
                covered = [];
                _coveredByFile[relativePath] = covered;
            }
        }

        foreach (var kvp in data.Files)
        {
            var relativePath = normalizer.Normalize(kvp.Value);

            if (!_coveredByFile.ContainsKey(relativePath))
                _coveredByFile[relativePath] = [];
        }
    }

    public Dictionary<string, (SortedSet<int> Covered, SortedSet<int> Uncovered)> GetMerged()
    {
        var allPaths = _coveredByFile.Keys
            .Concat(_uncoveredByFile.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, (SortedSet<int>, SortedSet<int>)>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in allPaths)
        {
            _coveredByFile.TryGetValue(path, out var covered);
            _uncoveredByFile.TryGetValue(path, out var uncovered);

            covered ??= [];
            uncovered ??= [];

            // A line covered in any report is covered
            covered.ExceptWith(uncovered);

            result[path] = (covered, uncovered);
        }

        return result;
    }
}
