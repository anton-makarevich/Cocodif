using Sanet.Cocodif.Services;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Services;

public class PathNormalizerTests
{
    [Fact]
    public void Normalize_AbsolutePath_ReturnsRelative()
    {
        var normalizer = new PathNormalizer("/home/user/repo");
        var result = normalizer.Normalize("/home/user/repo/src/Program.cs");
        result.ShouldBe("src/Program.cs");
    }

    [Fact]
    public void Normalize_DifferentCase_HandlesCorrectly()
    {
        var normalizer = new PathNormalizer("/home/user/Repo");
        var result = normalizer.Normalize("/home/user/repo/src/Program.cs");
        result.ShouldBe("src/Program.cs");
    }

    [Fact]
    public void Normalize_FileOutsideRoot_ReturnsFileName()
    {
        var normalizer = new PathNormalizer("/home/user/repo");
        var result = normalizer.Normalize("/tmp/some-other/file.cs");
        result.ShouldBe("file.cs");
    }
}
