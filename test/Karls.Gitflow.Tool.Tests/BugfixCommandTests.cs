using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class BugfixCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public BugfixCommandTests() {
        _repo = new GitRepositoryFixture();
        // Initialize gitflow for all bugfix tests
        _repo.ExecuteGitFlow("init -d");
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    #region List

    [Fact]
    public void BugfixList_WhenNoBugfixes_ReturnsEmpty() {
        // Act
        var result = _repo.ExecuteGitFlow("bugfix list");

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void BugfixList_WithBugfixes_ListsAllBugfixes() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.ExecuteGit("checkout develop");
        _repo.ExecuteGitFlow("bugfix start fix-456");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("fix-123");
        result.Output.ShouldContain("fix-456");
    }

    #endregion

    #region Start

    [Fact]
    public void BugfixStart_CreatesBugfixBranch() {
        // Act
        var result = _repo.ExecuteGitFlow("bugfix start fix-123");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("bugfix/fix-123").ShouldBeTrue();
    }

    [Fact]
    public void BugfixStart_ChecksOutBugfixBranch() {
        // Act
        _repo.ExecuteGitFlow("bugfix start fix-123");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("bugfix/fix-123");
    }

    [Fact]
    public void BugfixStart_CreatesFromDevelop() {
        // Arrange - Add a commit to develop
        _repo.ExecuteGit("checkout develop");
        _repo.CreateCommit("Develop commit");
        var developCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("bugfix start fix-123");

        // Assert - Bugfix branch should be based on develop
        var bugfixBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        bugfixBase.ShouldBe(developCommit);
    }

    [Fact]
    public void BugfixStart_WithCustomBase_CreatesFromBase() {
        // Arrange - Create a custom branch
        _repo.ExecuteGit("checkout -b custom-base");
        _repo.CreateCommit("Custom base commit");
        var customCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("bugfix start fix-123 custom-base");

        // Assert
        var bugfixBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        bugfixBase.ShouldBe(customCommit);
    }

    [Fact]
    public void BugfixStart_WhenBranchExists_ReturnsError() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix start fix-123");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already exists");
    }

    #endregion

    #region Finish

    [Fact]
    public void BugfixFinish_MergesIntoDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");
        var bugfixCommitMessage = "Bugfix work";

        // Act
        var result = _repo.ExecuteGitFlow("bugfix finish fix-123");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("develop");

        // Verify the bugfix commit is in develop
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain(bugfixCommitMessage);
    }

    [Fact]
    public void BugfixFinish_DeletesBugfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");

        // Act
        _repo.ExecuteGitFlow("bugfix finish fix-123");

        // Assert
        _repo.BranchExists("bugfix/fix-123").ShouldBeFalse();
    }

    [Fact]
    public void BugfixFinish_WithKeep_KeepsBugfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");

        // Act
        _repo.ExecuteGitFlow("bugfix finish fix-123 --keep");

        // Assert
        _repo.BranchExists("bugfix/fix-123").ShouldBeTrue();
    }

    [Fact]
    public void BugfixFinish_WhenOnBugfixBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");

        // Act - Don't specify the name, should auto-detect
        var result = _repo.ExecuteGitFlow("bugfix finish");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("develop");
        _repo.BranchExists("bugfix/fix-123").ShouldBeFalse();
    }

    [Fact]
    public void BugfixFinish_WithSquash_SquashesCommits() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("First commit");
        _repo.CreateCommit("Second commit");
        _repo.CreateCommit("Third commit");

        // Get develop commit count before
        _repo.ExecuteGit("checkout develop");
        var beforeLog = _repo.ExecuteGit("log --oneline");
        var beforeCount = beforeLog.Output.Split('\n').Length;
        _repo.ExecuteGit("checkout bugfix/fix-123");

        // Act
        _repo.ExecuteGitFlow("bugfix finish fix-123 --squash");

        // Assert - Should have only 1 new commit (the squash)
        var afterLog = _repo.ExecuteGit("log --oneline");
        var afterCount = afterLog.Output.Split('\n').Length;

        // One squash commit + one merge commit
        (afterCount - beforeCount).ShouldBeLessThanOrEqualTo(2);
    }

    #endregion

    #region Delete

    [Fact]
    public void BugfixDelete_DeletesBugfixBranch() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix delete fix-123 -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("bugfix/fix-123").ShouldBeFalse();
    }

    [Fact]
    public void BugfixDelete_WhenOnBugfixBranch_ChecksOutDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");

        // Act
        _repo.ExecuteGitFlow("bugfix delete fix-123 -f");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("develop");
    }

    [Fact]
    public void BugfixDelete_WhenOnBugfixBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("bugfix delete -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("bugfix/fix-123").ShouldBeFalse();
    }

    #endregion
}
