using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class SupportCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public SupportCommandTests() {
        _repo = new GitRepositoryFixture();
        // Initialize gitflow for all support tests
        _repo.ExecuteGitFlow("init -d");
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    #region List

    [Fact]
    public void SupportList_WhenNoSupport_ReturnsEmpty() {
        // Act
        var result = _repo.ExecuteGitFlow("support list");

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void SupportList_WithSupportBranches_ListsAllSupport() {
        // Arrange - Create tags to base support branches on
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");
        _repo.CreateCommit("Another commit");
        _repo.ExecuteGit("tag v2.0.0");

        _repo.ExecuteGitFlow("support start 1.x v1.0.0");
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGitFlow("support start 2.x v2.0.0");

        // Act
        var result = _repo.ExecuteGitFlow("support list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("1.x");
        result.Output.ShouldContain("2.x");
    }

    #endregion

    #region Start

    [Fact]
    public void SupportStart_CreatesSupportBranch() {
        // Arrange - Create a tag to base support branch on
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");

        // Act
        var result = _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeTrue();
    }

    [Fact]
    public void SupportStart_ChecksOutSupportBranch() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");

        // Act
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("support/1.x");
    }

    [Fact]
    public void SupportStart_CreatesFromTag() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        var mainCommit = _repo.ExecuteGit("rev-parse HEAD").Output;
        _repo.ExecuteGit("tag v1.0.0");

        // Act
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Assert - Support branch should be based on the tag
        var supportBase = _repo.ExecuteGit("rev-parse HEAD").Output;
        supportBase.ShouldBe(mainCommit);
    }

    [Fact]
    public void SupportStart_CreatesFromCommitHash() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        var commitHash = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        var result = _repo.ExecuteGitFlow($"support start 1.x {commitHash}");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeTrue();
    }

    [Fact]
    public void SupportStart_WhenBranchExists_ReturnsError() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");
        _repo.ExecuteGit("checkout main");

        // Act
        var result = _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already exists");
    }

    [Fact]
    public void SupportStart_WithInvalidBase_ReturnsError() {
        // Act
        var result = _repo.ExecuteGitFlow("support start 1.x nonexistent-tag");

        // Assert
        result.Success.ShouldBeFalse();
    }

    #endregion

    #region Delete

    [Fact]
    public void SupportDelete_DeletesSupportBranch() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");
        _repo.ExecuteGit("checkout main");

        // Act
        var result = _repo.ExecuteGitFlow("support delete 1.x -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeFalse();
    }

    [Fact]
    public void SupportDelete_WhenOnSupportBranch_ChecksOutMain() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Act
        _repo.ExecuteGitFlow("support delete 1.x -f");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("main");
    }

    [Fact]
    public void SupportDelete_WhenOnSupportBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag v1.0.0");
        _repo.ExecuteGitFlow("support start 1.x v1.0.0");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("support delete -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeFalse();
    }

    #endregion
}
