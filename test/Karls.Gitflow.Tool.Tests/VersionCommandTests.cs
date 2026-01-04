using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class VersionCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public VersionCommandTests() {
        _repo = new GitRepositoryFixture();
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Version_ShowsVersionInfo() {
        // Act
        var result = _repo.ExecuteGitFlow("version");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("git-flow");
        result.Output.ShouldContain("version");
    }

    [Fact]
    public void Version_ShowsDescription() {
        // Act
        var result = _repo.ExecuteGitFlow("version");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("gitflow");
    }

    [Fact]
    public void Version_WorksWithoutGitFlowInit() {
        // Version command should work even without gitflow being initialized
        // Act
        var result = _repo.ExecuteGitFlow("version");

        // Assert
        result.Success.ShouldBeTrue();
    }
}
