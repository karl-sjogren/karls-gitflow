using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class ReleaseCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public ReleaseCommandTests() {
        _repo = new GitRepositoryFixture();
        // Initialize gitflow for all release tests
        _repo.ExecuteGitFlow("init -d");
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    #region List

    [Fact]
    public void ReleaseList_WhenNoReleases_ReturnsEmpty() {
        // Act
        var result = _repo.ExecuteGitFlow("release list");

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void ReleaseList_WithReleases_ListsAllReleases() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.ExecuteGit("checkout develop");
        _repo.ExecuteGitFlow("release start 2.0.0");

        // Act
        var result = _repo.ExecuteGitFlow("release list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("1.0.0");
        result.Output.ShouldContain("2.0.0");
    }

    #endregion

    #region Start

    [Fact]
    public void ReleaseStart_CreatesReleaseBranch() {
        // Act
        var result = _repo.ExecuteGitFlow("release start 1.0.0");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("release/1.0.0").ShouldBeTrue();
    }

    [Fact]
    public void ReleaseStart_ChecksOutReleaseBranch() {
        // Act
        _repo.ExecuteGitFlow("release start 1.0.0");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("release/1.0.0");
    }

    [Fact]
    public void ReleaseStart_CreatesFromDevelop() {
        // Arrange - Add a commit to develop
        _repo.ExecuteGit("checkout develop");
        _repo.CreateCommit("Develop commit for release");
        var developCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("release start 1.0.0");

        // Assert - Release branch should be based on develop
        var releaseBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        releaseBase.ShouldBe(developCommit);
    }

    #endregion

    #region Finish

    [Fact]
    public void ReleaseFinish_MergesIntoMain() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        var result = _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify merge into main
        _repo.ExecuteGit("checkout main");
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain("Release preparation");
    }

    [Fact]
    public void ReleaseFinish_MergesIntoDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert - Should be on develop after finish
        _repo.GetCurrentBranch().ShouldBe("develop");

        // Verify merge into develop
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain("Release preparation");
    }

    [Fact]
    public void ReleaseFinish_CreatesTag() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert
        _repo.TagExists("1.0.0").ShouldBeTrue();
    }

    [Fact]
    public void ReleaseFinish_WithVersionTagPrefix_CreatesTagWithPrefix() {
        // Arrange - Set version tag prefix
        _repo.ExecuteGit("config gitflow.prefix.versiontag v");
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert
        _repo.TagExists("v1.0.0").ShouldBeTrue();
    }

    [Fact]
    public void ReleaseFinish_DeletesReleaseBranch() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert
        _repo.BranchExists("release/1.0.0").ShouldBeFalse();
    }

    [Fact]
    public void ReleaseFinish_WithKeep_KeepsReleaseBranch() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0 --keep");

        // Assert
        _repo.BranchExists("release/1.0.0").ShouldBeTrue();
    }

    [Fact]
    public void ReleaseFinish_WithNoTag_SkipsTagCreation() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0 --notag");

        // Assert
        _repo.TagExists("1.0.0").ShouldBeFalse();
    }

    [Fact]
    public void ReleaseFinish_WithCustomMessage_UsesMessageForTag() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0 -m \"Custom tag message\"");

        // Assert
        _repo.TagExists("1.0.0").ShouldBeTrue();

        // Verify tag message
        var tagMessage = _repo.ExecuteGit("tag -l -n1 1.0.0");
        tagMessage.Output.ShouldContain("Custom tag message");
    }

    [Fact]
    public void ReleaseFinish_WhenOnReleaseBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("release finish");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.TagExists("1.0.0").ShouldBeTrue();
        _repo.BranchExists("release/1.0.0").ShouldBeFalse();
    }

    [Fact]
    public void ReleaseFinish_ReturnsToDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release preparation");

        // Act
        _repo.ExecuteGitFlow("release finish 1.0.0");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("develop");
    }

    #endregion

    #region Delete

    [Fact]
    public void ReleaseDelete_DeletesReleaseBranch() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("release delete 1.0.0 -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("release/1.0.0").ShouldBeFalse();
    }

    #endregion
}
