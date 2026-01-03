using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class HotfixCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public HotfixCommandTests() {
        _repo = new GitRepositoryFixture();
        // Initialize gitflow for all hotfix tests
        _repo.ExecuteGitFlow("init -d");
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    #region List

    [Fact]
    public void HotfixList_WhenNoHotfixes_ReturnsEmpty() {
        // Act
        var result = _repo.ExecuteGitFlow("hotfix list");

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void HotfixList_WithHotfixes_ListsAllHotfixes() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGitFlow("hotfix start 1.0.2");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("1.0.1");
        result.Output.ShouldContain("1.0.2");
    }

    #endregion

    #region Start

    [Fact]
    public void HotfixStart_CreatesHotfixBranch() {
        // Act
        var result = _repo.ExecuteGitFlow("hotfix start 1.0.1");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("hotfix/1.0.1").ShouldBeTrue();
    }

    [Fact]
    public void HotfixStart_ChecksOutHotfixBranch() {
        // Act
        _repo.ExecuteGitFlow("hotfix start 1.0.1");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("hotfix/1.0.1");
    }

    [Fact]
    public void HotfixStart_CreatesFromMain() {
        // Arrange - Add a commit to main
        _repo.ExecuteGit("checkout main");
        _repo.CreateCommit("Main commit for hotfix");
        var mainCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("hotfix start 1.0.1");

        // Assert - Hotfix branch should be based on main (not develop)
        var hotfixBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        hotfixBase.ShouldBe(mainCommit);
    }

    #endregion

    #region Finish

    [Fact]
    public void HotfixFinish_MergesIntoMain() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify merge into main
        _repo.ExecuteGit("checkout main");
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain("Hotfix work");
    }

    [Fact]
    public void HotfixFinish_MergesIntoDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert - Should be on develop after finish
        _repo.GetCurrentBranch().ShouldBe("develop");

        // Verify merge into develop
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain("Hotfix work");
    }

    [Fact]
    public void HotfixFinish_CreatesTag() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert
        _repo.TagExists("1.0.1").ShouldBeTrue();
    }

    [Fact]
    public void HotfixFinish_WithVersionTagPrefix_CreatesTagWithPrefix() {
        // Arrange - Set version tag prefix
        _repo.ExecuteGit("config gitflow.prefix.versiontag v");
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert
        _repo.TagExists("v1.0.1").ShouldBeTrue();
    }

    [Fact]
    public void HotfixFinish_DeletesHotfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert
        _repo.BranchExists("hotfix/1.0.1").ShouldBeFalse();
    }

    [Fact]
    public void HotfixFinish_WithKeep_KeepsHotfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1 --keep");

        // Assert
        _repo.BranchExists("hotfix/1.0.1").ShouldBeTrue();
    }

    [Fact]
    public void HotfixFinish_WithNoTag_SkipsTagCreation() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1 --notag");

        // Assert
        _repo.TagExists("1.0.1").ShouldBeFalse();
    }

    [Fact]
    public void HotfixFinish_WhenOnHotfixBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("hotfix finish");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.TagExists("1.0.1").ShouldBeTrue();
        _repo.BranchExists("hotfix/1.0.1").ShouldBeFalse();
    }

    [Fact]
    public void HotfixFinish_ReturnsToDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        _repo.ExecuteGitFlow("hotfix finish 1.0.1");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("develop");
    }

    #endregion

    #region Delete

    [Fact]
    public void HotfixDelete_DeletesHotfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.ExecuteGit("checkout main");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix delete 1.0.1 -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("hotfix/1.0.1").ShouldBeFalse();
    }

    #endregion
}
