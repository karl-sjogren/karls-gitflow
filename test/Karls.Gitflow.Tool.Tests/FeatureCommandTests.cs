using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class FeatureCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public FeatureCommandTests() {
        _repo = new GitRepositoryFixture();
        // Initialize gitflow for all feature tests
        _repo.ExecuteGitFlow("init -d");
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    #region List

    [Fact]
    public void FeatureList_WhenNoFeatures_ReturnsEmpty() {
        // Act
        var result = _repo.ExecuteGitFlow("feature list");

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void FeatureList_WithFeatures_ListsAllFeatures() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.ExecuteGit("checkout develop");
        _repo.ExecuteGitFlow("feature start another-feature");

        // Act
        var result = _repo.ExecuteGitFlow("feature list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("my-feature");
        result.Output.ShouldContain("another-feature");
    }

    #endregion

    #region Start

    [Fact]
    public void FeatureStart_CreatesFeatureBranch() {
        // Act
        var result = _repo.ExecuteGitFlow("feature start my-feature");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
    }

    [Fact]
    public void FeatureStart_ChecksOutFeatureBranch() {
        // Act
        _repo.ExecuteGitFlow("feature start my-feature");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("feature/my-feature");
    }

    [Fact]
    public void FeatureStart_CreatesFromDevelop() {
        // Arrange - Add a commit to develop
        _repo.ExecuteGit("checkout develop");
        _repo.CreateCommit("Develop commit");
        var developCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("feature start my-feature");

        // Assert - Feature branch should be based on develop
        var featureBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        featureBase.ShouldBe(developCommit);
    }

    [Fact]
    public void FeatureStart_WithCustomBase_CreatesFromBase() {
        // Arrange - Create a custom branch
        _repo.ExecuteGit("checkout -b custom-base");
        _repo.CreateCommit("Custom base commit");
        var customCommit = _repo.ExecuteGit("rev-parse HEAD").Output;

        // Act
        _repo.ExecuteGitFlow("feature start my-feature custom-base");

        // Assert
        var featureBase = _repo.ExecuteGit("rev-parse HEAD~0").Output;
        featureBase.ShouldBe(customCommit);
    }

    [Fact]
    public void FeatureStart_WhenBranchExists_ReturnsError() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("feature start my-feature");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already exists");
    }

    #endregion

    #region Finish

    [Fact]
    public void FeatureFinish_MergesIntoDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");
        var featureCommitMessage = "Feature work";

        // Act
        var result = _repo.ExecuteGitFlow("feature finish my-feature");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("develop");

        // Verify the feature commit is in develop
        var log = _repo.ExecuteGit("log --oneline");
        log.Output.ShouldContain(featureCommitMessage);
    }

    [Fact]
    public void FeatureFinish_DeletesFeatureBranch() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");

        // Act
        _repo.ExecuteGitFlow("feature finish my-feature");

        // Assert
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    [Fact]
    public void FeatureFinish_WithKeep_KeepsFeatureBranch() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");

        // Act
        _repo.ExecuteGitFlow("feature finish my-feature --keep");

        // Assert
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
    }

    [Fact]
    public void FeatureFinish_WhenOnFeatureBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");

        // Act - Don't specify the name, should auto-detect
        var result = _repo.ExecuteGitFlow("feature finish");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("develop");
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    [Fact]
    public void FeatureFinish_WithSquash_SquashesCommits() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("First commit");
        _repo.CreateCommit("Second commit");
        _repo.CreateCommit("Third commit");

        // Get develop commit count before
        _repo.ExecuteGit("checkout develop");
        var beforeLog = _repo.ExecuteGit("log --oneline");
        var beforeCount = beforeLog.Output.Split('\n').Length;
        _repo.ExecuteGit("checkout feature/my-feature");

        // Act
        _repo.ExecuteGitFlow("feature finish my-feature --squash");

        // Assert - Should have only 1 new commit (the squash)
        var afterLog = _repo.ExecuteGit("log --oneline");
        var afterCount = afterLog.Output.Split('\n').Length;

        // One squash commit + one merge commit
        (afterCount - beforeCount).ShouldBeLessThanOrEqualTo(2);
    }

    #endregion

    #region Delete

    [Fact]
    public void FeatureDelete_DeletesFeatureBranch() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("feature delete my-feature -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    [Fact]
    public void FeatureDelete_WhenOnFeatureBranch_ChecksOutDevelop() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");

        // Act
        _repo.ExecuteGitFlow("feature delete my-feature -f");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("develop");
    }

    [Fact]
    public void FeatureDelete_WhenOnFeatureBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("feature delete -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    #endregion
}
