using Sanet.Cocodif.Services;
using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests.Services;

public class GlobMatcherTests
{
    [Theory]
    [InlineData("src/Program.cs", "**/*", true)]
    [InlineData("src/Program.cs", "src/**", true)]
    [InlineData("src/Program.cs", "lib/**", false)]
    [InlineData("src/app/Program.cs", "src/**/*.cs", true)]
    [InlineData("tests/Program.cs", "src/**", false)]
    public void Matches_VariousGlobs_ReturnsExpected(string path, string pattern, bool expected)
    {
        GlobMatcher.Matches(path, pattern).ShouldBe(expected);
    }

    [Theory]
    [InlineData("src/obj/debug.dll", "**/obj/**", true)]
    [InlineData("src/bin/release.dll", "**/bin/**", true)]
    [InlineData("src/Program.cs", "**/obj/**", false)]
    [InlineData("src/Program.cs", "**/bin/**", false)]
    [InlineData("src/Program.cs", "**/obj/**,**/bin/**", false)]
    public void IsExcluded_DefaultExcludes_WorkCorrectly(string path, string pattern, bool expected)
    {
        GlobMatcher.IsExcluded(path, pattern).ShouldBe(expected);
    }

    [Fact]
    public void IsExcluded_EmptyPattern_ReturnsFalse()
    {
        GlobMatcher.IsExcluded("src/Program.cs", "").ShouldBeFalse();
    }

    [Fact]
    public void Matches_EmptyPattern_ReturnsTrue()
    {
        GlobMatcher.Matches("src/Program.cs", "").ShouldBeTrue();
    }
}
