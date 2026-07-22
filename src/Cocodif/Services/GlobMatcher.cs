namespace Sanet.Cocodif.Services;

public static class GlobMatcher
{
    public static bool Matches(string relativePath, string globPattern)
    {
        if (string.IsNullOrWhiteSpace(globPattern) || globPattern == "**/*")
            return true;

        var patterns = globPattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return patterns.Any(p => SingleMatch(relativePath, p));
    }

    public static bool IsExcluded(string relativePath, string excludePattern)
    {
        if (string.IsNullOrWhiteSpace(excludePattern))
            return false;

        var patterns = excludePattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return patterns.Any(p => SingleMatch(relativePath, p));
    }

    private static bool SingleMatch(string relativePath, string pattern)
    {
        var normalizedPath = relativePath.Replace('\\', '/').ToLowerInvariant();
        var normalizedPattern = pattern.Replace('\\', '/').ToLowerInvariant();

        // Convert glob pattern to regex
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(normalizedPattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", "[^/]") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(normalizedPath, regexPattern);
    }
}
