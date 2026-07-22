namespace Sanet.Cocodif.Services;

public class PathNormalizer
{
    private readonly string _root;

    public PathNormalizer(string root)
    {
        _root = NormalizeSeparators(Path.GetFullPath(root));
    }

    public string Normalize(string path)
    {
        var full = NormalizeSeparators(Path.GetFullPath(path));

        if (full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            var relative = full[_root.Length..].TrimStart('/', '\\');
            return relative;
        }

        return Path.GetFileName(full);
    }

    private static string NormalizeSeparators(string path)
        => path.Replace('\\', '/');
}
