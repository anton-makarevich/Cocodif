using Shouldly;
using Xunit;

namespace Sanet.Cocodif.Tests;

public class CliIntegrationTests
{
    [Fact]
    public async Task RunAsync_MissingCoverageFile_ReturnsExitCode3()
    {
        var result = await Program.RunAsync(
            [new FileInfo("nonexistent.xml")],
            "auto",
            new FileInfo("nonexistent.txt"),
            null,
            "**/*",
            "",
            "Test",
            "markdown",
            null,
            null);

        result.ShouldBe(3);
    }

    [Fact]
    public async Task RunAsync_MissingChangedFiles_ReturnsExitCode3()
    {
        var tempDir = Path.GetTempPath();
        var covPath = Path.Combine(tempDir, $"test-cov-{Guid.NewGuid():N}.xml");
        await File.WriteAllTextAsync(covPath, """
            <?xml version="1.0" encoding="utf-8"?>
            <CoverageSession><Modules></Modules></CoverageSession>
            """);

        try
        {
            var result = await Program.RunAsync(
                [new FileInfo(covPath)],
                "auto",
                new FileInfo("nonexistent.txt"),
                null,
                "**/*",
                "",
                "Test",
                "markdown",
                null,
                null);

            result.ShouldBe(3);
        }
        finally
        {
            File.Delete(covPath);
        }
    }

    [Fact]
    public async Task RunAsync_InvalidFormat_ReturnsExitCode2()
    {
        var result = await Program.RunAsync(
            [],
            "badformat",
            new FileInfo("nonexistent.txt"),
            null,
            "**/*",
            "",
            "Test",
            "markdown",
            null,
            null);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task RunAsync_FailUnder_BelowThreshold_ReturnsExitCode1()
    {
        var tempDir = Path.GetTempPath();
        var guid = Guid.NewGuid().ToString("N");
        var covPath = Path.Combine(tempDir, $"cov-{guid}.xml");
        var changedPath = Path.Combine(tempDir, $"changed-{guid}.txt");
        var outputPath = Path.Combine(tempDir, $"report-{guid}.md");

        await File.WriteAllTextAsync(covPath, """
            <?xml version="1.0" encoding="utf-8"?>
            <CoverageSession>
              <Modules>
                <Module>
                  <Files>
                    <File uid="1" fullPath="/repo/src/A.cs" />
                  </Files>
                  <Classes>
                    <Class>
                      <Methods>
                        <Method>
                          <SequencePoints>
                            <SequencePoint vc="0" fileid="1" sl="1" />
                            <SequencePoint vc="0" fileid="1" sl="2" />
                          </SequencePoints>
                        </Method>
                      </Methods>
                    </Class>
                  </Classes>
                </Module>
              </Modules>
            </CoverageSession>
            """);

        await File.WriteAllTextAsync(changedPath, "src/A.cs");

        try
        {
            var result = await Program.RunAsync(
                [new FileInfo(covPath)],
                "auto",
                new FileInfo(changedPath),
                new DirectoryInfo("/repo"),
                "**/*",
                "",
                "Test",
                "markdown",
                new FileInfo(outputPath),
                80.0);

            result.ShouldBe(1);
        }
        finally
        {
            File.Delete(covPath);
            File.Delete(changedPath);
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task RunAsync_FailUnder_AboveThreshold_ReturnsExitCode0()
    {
        var tempDir = Path.GetTempPath();
        var guid = Guid.NewGuid().ToString("N");
        var covPath = Path.Combine(tempDir, $"cov-{guid}.xml");
        var changedPath = Path.Combine(tempDir, $"changed-{guid}.txt");
        var outputPath = Path.Combine(tempDir, $"report-{guid}.md");

        await File.WriteAllTextAsync(covPath, """
            <?xml version="1.0" encoding="utf-8"?>
            <CoverageSession>
              <Modules>
                <Module>
                  <Files>
                    <File uid="1" fullPath="/repo/src/A.cs" />
                  </Files>
                  <Classes>
                    <Class>
                      <Methods>
                        <Method>
                          <SequencePoints>
                            <SequencePoint vc="1" fileid="1" sl="1" />
                            <SequencePoint vc="1" fileid="1" sl="2" />
                          </SequencePoints>
                        </Method>
                      </Methods>
                    </Class>
                  </Classes>
                </Module>
              </Modules>
            </CoverageSession>
            """);

        await File.WriteAllTextAsync(changedPath, "src/A.cs");

        try
        {
            var result = await Program.RunAsync(
                [new FileInfo(covPath)],
                "auto",
                new FileInfo(changedPath),
                new DirectoryInfo("/repo"),
                "**/*",
                "",
                "Test",
                "markdown",
                new FileInfo(outputPath),
                80.0);

            result.ShouldBe(0);
        }
        finally
        {
            File.Delete(covPath);
            File.Delete(changedPath);
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task RunAsync_MultipleCoverageFiles_MergesResults()
    {
        var tempDir = Path.GetTempPath();
        var guid = Guid.NewGuid().ToString("N");
        var cov1Path = Path.Combine(tempDir, $"cov1-{guid}.xml");
        var cov2Path = Path.Combine(tempDir, $"cov2-{guid}.xml");
        var changedPath = Path.Combine(tempDir, $"changed-{guid}.txt");
        var outputPath = Path.Combine(tempDir, $"report-{guid}.md");

        await File.WriteAllTextAsync(cov1Path, """
            <?xml version="1.0" encoding="utf-8"?>
            <CoverageSession>
              <Modules>
                <Module>
                  <Files>
                    <File uid="1" fullPath="/repo/src/A.cs" />
                  </Files>
                  <Classes>
                    <Class>
                      <Methods>
                        <Method>
                          <SequencePoints>
                            <SequencePoint vc="0" fileid="1" sl="10" />
                          </SequencePoints>
                        </Method>
                      </Methods>
                    </Class>
                  </Classes>
                </Module>
              </Modules>
            </CoverageSession>
            """);

        await File.WriteAllTextAsync(cov2Path, """
            <?xml version="1.0" encoding="utf-8"?>
            <CoverageSession>
              <Modules>
                <Module>
                  <Files>
                    <File uid="1" fullPath="/repo/src/B.cs" />
                  </Files>
                  <Classes>
                    <Class>
                      <Methods>
                        <Method>
                          <SequencePoints>
                            <SequencePoint vc="0" fileid="1" sl="5" />
                          </SequencePoints>
                        </Method>
                      </Methods>
                    </Class>
                  </Classes>
                </Module>
              </Modules>
            </CoverageSession>
            """);

        await File.WriteAllTextAsync(changedPath, "src/A.cs\nsrc/B.cs");

        try
        {
            var result = await Program.RunAsync(
                [new FileInfo(cov1Path), new FileInfo(cov2Path)],
                "auto",
                new FileInfo(changedPath),
                new DirectoryInfo("/repo"),
                "**/*",
                "",
                "Test",
                "markdown",
                new FileInfo(outputPath),
                null);

            result.ShouldBe(0);

            var content = await File.ReadAllTextAsync(outputPath);
            content.ShouldContain("src/A.cs");
            content.ShouldContain("src/B.cs");
        }
        finally
        {
            File.Delete(cov1Path);
            File.Delete(cov2Path);
            File.Delete(changedPath);
            File.Delete(outputPath);
        }
    }
}
