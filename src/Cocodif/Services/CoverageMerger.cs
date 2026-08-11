using Sanet.Cocodif.Models;

namespace Sanet.Cocodif.Services;

public class CoverageMerger
{
    private readonly Dictionary<string, SortedSet<int>> _coveredByFile = new();
    private readonly Dictionary<string, SortedSet<int>> _uncoveredByFile = new();

    public void Add(CoverageData data, PathNormalizer normalizer)
    {
        AddGrouped(data.UncoveredLines, _uncoveredByFile, normalizer);
        AddGrouped(data.CoveredLines, _coveredByFile, normalizer);

        foreach (var kvp in data.Files)
        {
            var relativePath = normalizer.Normalize(kvp.Value);

            if (!_coveredByFile.ContainsKey(relativePath))
                _coveredByFile[relativePath] = [];
        }
    }

    private static void AddGrouped(
        List<SourceLine> lines,
        Dictionary<string, SortedSet<int>> byFile,
        PathNormalizer normalizer)
    {
        foreach (var group in lines.GroupBy(ul => normalizer.Normalize(ul.FilePath)))
        {
            if (!byFile.TryGetValue(group.Key, out var lineNumbers))
            {
                lineNumbers = [];
                byFile[group.Key] = lineNumbers;
            }

            foreach (var line in group)
                lineNumbers.Add(line.LineNumber);
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
            uncovered.ExceptWith(covered);

            result[path] = (covered, uncovered);
        }

        return result;
    }
}
