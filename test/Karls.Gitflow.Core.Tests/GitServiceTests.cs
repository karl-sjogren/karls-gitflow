namespace Karls.Gitflow.Core.Tests;

public class GitServiceTests {
    private readonly IGitExecutor _fakeExecutor;
    private readonly GitService _sut;

    public GitServiceTests() {
        _fakeExecutor = A.Fake<IGitExecutor>();
        _sut = new GitService(_fakeExecutor);
    }

    #region IsGitRepository

    [Fact]
    public void IsGitRepository_WhenInsideGitRepo_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("rev-parse --is-inside-work-tree"))
            .Returns(new GitExecutorResult(["true"], 0));

        // Act
        var result = _sut.IsGitRepository();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsGitRepository_WhenNotInsideGitRepo_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("rev-parse --is-inside-work-tree"))
            .Returns(new GitExecutorResult([], 128));

        // Act
        var result = _sut.IsGitRepository();

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsWorkingTreeClean

    [Fact]
    public void IsWorkingTreeClean_WhenClean_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("status --porcelain"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.IsWorkingTreeClean();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsWorkingTreeClean_WhenDirty_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("status --porcelain"))
            .Returns(new GitExecutorResult(["M  file.txt"], 0));

        // Act
        var result = _sut.IsWorkingTreeClean();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsWorkingTreeClean_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("status --porcelain"))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.IsWorkingTreeClean());
    }

    #endregion

    #region GetCurrentBranchName

    [Fact]
    public void GetCurrentBranchName_ReturnsCurrentBranch() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("rev-parse --abbrev-ref HEAD"))
            .Returns(new GitExecutorResult(["develop"], 0));

        // Act
        var result = _sut.GetCurrentBranchName();

        // Assert
        result.ShouldBe("develop");
    }

    [Fact]
    public void GetCurrentBranchName_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("rev-parse --abbrev-ref HEAD"))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetCurrentBranchName());
    }

    #endregion

    #region GetLocalBranches

    [Fact]
    public void GetLocalBranches_ReturnsBranches() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("for-each-ref --sort=refname --format=%(refname:short) refs/heads"))
            .Returns(new GitExecutorResult(["develop", "main", "feature/test"], 0));

        // Act
        var result = _sut.GetLocalBranches();

        // Assert
        result.ShouldBe(["develop", "main", "feature/test"]);
    }

    #endregion

    #region LocalBranchExists

    [Fact]
    public void LocalBranchExists_WhenBranchExists_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("show-ref --verify --quiet refs/heads/develop"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.LocalBranchExists("develop");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void LocalBranchExists_WhenBranchDoesNotExist_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("show-ref --verify --quiet refs/heads/nonexistent"))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.LocalBranchExists("nonexistent");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region RemoteBranchExists

    [Fact]
    public void RemoteBranchExists_WhenBranchExists_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("show-ref --verify --quiet refs/remotes/origin/develop"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.RemoteBranchExists("develop");

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region TagExists

    [Fact]
    public void TagExists_WhenTagExists_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("show-ref --verify --quiet refs/tags/v1.0.0"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.TagExists("v1.0.0");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void TagExists_WhenTagDoesNotExist_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("show-ref --verify --quiet refs/tags/v1.0.0"))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.TagExists("v1.0.0");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsGitFlowInitialized

    [Fact]
    public void IsGitFlowInitialized_WhenConfigured_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("config --get gitflow.branch.master"))
            .Returns(new GitExecutorResult(["main"], 0));
        A.CallTo(() => _fakeExecutor.Execute("config --get gitflow.branch.develop"))
            .Returns(new GitExecutorResult(["develop"], 0));

        // Act
        var result = _sut.IsGitFlowInitialized();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsGitFlowInitialized_WhenNotConfigured_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("config --get gitflow.branch.master"))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.IsGitFlowInitialized();

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region GetGitFlowConfiguration

    [Fact]
    public void GetGitFlowConfiguration_ReturnsConfiguredValues() {
        // Arrange
        SetupConfigGet("gitflow.branch.master", "main");
        SetupConfigGet("gitflow.branch.develop", "develop");
        SetupConfigGet("gitflow.prefix.feature", "feature/");
        SetupConfigGet("gitflow.prefix.bugfix", "bugfix/");
        SetupConfigGet("gitflow.prefix.release", "release/");
        SetupConfigGet("gitflow.prefix.hotfix", "hotfix/");
        SetupConfigGet("gitflow.prefix.support", "support/");
        SetupConfigGet("gitflow.prefix.versiontag", "v");

        // Act
        var result = _sut.GetGitFlowConfiguration();

        // Assert
        result.MainBranch.ShouldBe("main");
        result.DevelopBranch.ShouldBe("develop");
        result.FeaturePrefix.ShouldBe("feature/");
        result.BugfixPrefix.ShouldBe("bugfix/");
        result.ReleasePrefix.ShouldBe("release/");
        result.HotfixPrefix.ShouldBe("hotfix/");
        result.SupportPrefix.ShouldBe("support/");
        result.VersionTagPrefix.ShouldBe("v");
    }

    [Fact]
    public void GetGitFlowConfiguration_WhenNotConfigured_ReturnsDefaults() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string>.That.StartsWith("config --get")))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.GetGitFlowConfiguration();

        // Assert
        result.MainBranch.ShouldBe(GitFlowConfiguration.DefaultValues.MainBranch);
        result.DevelopBranch.ShouldBe(GitFlowConfiguration.DefaultValues.DevelopBranch);
        result.FeaturePrefix.ShouldBe(GitFlowConfiguration.DefaultValues.FeaturePrefix);
    }

    #endregion

    #region Branch Operations

    [Fact]
    public void CreateBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("checkout -b feature/test develop"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateBranch("feature/test", "develop");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("checkout -b feature/test develop"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CreateBranch_WhenFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string>.That.StartsWith("checkout -b")))
            .Returns(new GitExecutorResult(["error"], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.CreateBranch("feature/test", "develop"));
    }

    [Fact]
    public void CheckoutBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("checkout develop"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CheckoutBranch("develop");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("checkout develop"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteLocalBranch_WithoutForce_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("branch -d feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteLocalBranch("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("branch -d feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteLocalBranch_WithForce_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("branch -D feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteLocalBranch("feature/test", force: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("branch -D feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteRemoteBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("push origin --delete feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteRemoteBranch("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("push origin --delete feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Merge Operations

    [Fact]
    public void MergeBranch_WithNoFastForward_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("merge --no-ff feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.MergeBranch("feature/test", noFastForward: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("merge --no-ff feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void MergeBranch_WithFastForward_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("merge feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.MergeBranch("feature/test", noFastForward: false);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("merge feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Tag Operations

    [Fact]
    public void CreateTag_WithMessage_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("tag -a v1.0.0 -m \"Release 1.0.0\""))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateTag("v1.0.0", "Release 1.0.0");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("tag -a v1.0.0 -m \"Release 1.0.0\""))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CreateTag_WithoutMessage_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("tag v1.0.0"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateTag("v1.0.0");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("tag v1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Remote Operations

    [Fact]
    public void Fetch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("fetch origin"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.Fetch();

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("fetch origin"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PushBranch_WithSetUpstream_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("push -u origin feature/test"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.PushBranch("feature/test", setUpstream: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("push -u origin feature/test"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PushTags_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute("push origin --tags"))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.PushTags();

        // Assert
        A.CallTo(() => _fakeExecutor.Execute("push origin --tags"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Helper Methods

    private void SetupConfigGet(string key, string value) {
        A.CallTo(() => _fakeExecutor.Execute($"config --get {key}"))
            .Returns(new GitExecutorResult([value], 0));
    }

    #endregion
}
