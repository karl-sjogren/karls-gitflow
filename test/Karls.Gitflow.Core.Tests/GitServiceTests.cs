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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--is-inside-work-tree" }))))
            .Returns(new GitExecutorResult(["true"], 0));

        // Act
        var result = _sut.IsGitRepository();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsGitRepository_WhenNotInsideGitRepo_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--is-inside-work-tree" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "status", "--porcelain" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.IsWorkingTreeClean();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsWorkingTreeClean_WhenDirty_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "status", "--porcelain" }))))
            .Returns(new GitExecutorResult(["M  file.txt"], 0));

        // Act
        var result = _sut.IsWorkingTreeClean();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsWorkingTreeClean_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "status", "--porcelain" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.IsWorkingTreeClean());
    }

    #endregion

    #region GetCurrentBranchName

    [Fact]
    public void GetCurrentBranchName_ReturnsCurrentBranch() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--abbrev-ref", "HEAD" }))))
            .Returns(new GitExecutorResult(["develop"], 0));

        // Act
        var result = _sut.GetCurrentBranchName();

        // Assert
        result.ShouldBe("develop");
    }

    [Fact]
    public void GetCurrentBranchName_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--abbrev-ref", "HEAD" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetCurrentBranchName());
    }

    #endregion

    #region GetLocalBranches

    [Fact]
    public void GetLocalBranches_ReturnsBranches() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/heads" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "show-ref", "--verify", "--quiet", "refs/heads/develop" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.LocalBranchExists("develop");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void LocalBranchExists_WhenBranchDoesNotExist_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "show-ref", "--verify", "--quiet", "refs/heads/nonexistent" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "show-ref", "--verify", "--quiet", "refs/remotes/origin/develop" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "show-ref", "--verify", "--quiet", "refs/tags/v1.0.0" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.TagExists("v1.0.0");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void TagExists_WhenTagDoesNotExist_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "show-ref", "--verify", "--quiet", "refs/tags/v1.0.0" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--get", "gitflow.branch.master" }))))
            .Returns(new GitExecutorResult(["main"], 0));
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--get", "gitflow.branch.develop" }))))
            .Returns(new GitExecutorResult(["develop"], 0));

        // Act
        var result = _sut.IsGitFlowInitialized();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsGitFlowInitialized_WhenNotConfigured_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--get", "gitflow.branch.master" }))))
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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--list" }))))
            .Returns(new GitExecutorResult([
                "gitflow.branch.master=main",
                "gitflow.branch.develop=develop",
                "gitflow.prefix.feature=feature/",
                "gitflow.prefix.bugfix=bugfix/",
                "gitflow.prefix.release=release/",
                "gitflow.prefix.hotfix=hotfix/",
                "gitflow.prefix.support=support/",
                "gitflow.prefix.versiontag=v"
            ], 0));

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
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--list" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.GetGitFlowConfiguration();

        // Assert
        result.MainBranch.ShouldBe(GitFlowConfiguration.DefaultValues.MainBranch);
        result.DevelopBranch.ShouldBe(GitFlowConfiguration.DefaultValues.DevelopBranch);
        result.FeaturePrefix.ShouldBe(GitFlowConfiguration.DefaultValues.FeaturePrefix);
    }

    [Fact]
    public void GetGitFlowConfiguration_LaterEntriesOverrideEarlierOnes() {
        // Arrange - local config overrides global (same key appears twice)
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--list" }))))
            .Returns(new GitExecutorResult([
                "gitflow.branch.master=master",
                "gitflow.branch.master=main"
            ], 0));

        // Act
        var result = _sut.GetGitFlowConfiguration();

        // Assert - last value wins (local overrides global)
        result.MainBranch.ShouldBe("main");
    }

    #endregion

    #region Branch Operations

    [Fact]
    public void CreateBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "checkout", "-b", "feature/test", "develop" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateBranch("feature/test", "develop");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "checkout", "-b", "feature/test", "develop" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CreateBranch_WhenFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.Length >= 2 && a[0] == "checkout" && a[1] == "-b")))
            .Returns(new GitExecutorResult(["error"], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.CreateBranch("feature/test", "develop"));
    }

    [Fact]
    public void CheckoutBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "checkout", "develop" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CheckoutBranch("develop");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "checkout", "develop" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteLocalBranch_WithoutForce_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "-d", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteLocalBranch("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "-d", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteLocalBranch_WithForce_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "-D", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteLocalBranch("feature/test", force: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "-D", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteRemoteBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--delete", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteRemoteBranch("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--delete", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Merge Operations

    [Fact]
    public void MergeBranch_WithNoFastForward_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--no-ff", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.MergeBranch("feature/test", noFastForward: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--no-ff", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void MergeBranch_WithFastForward_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.MergeBranch("feature/test", noFastForward: false);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Tag Operations

    [Fact]
    public void CreateTag_WithMessage_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0", "-m", "Release 1.0.0" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateTag("v1.0.0", "Release 1.0.0");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0", "-m", "Release 1.0.0" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CreateTag_WithoutMessage_ExecutesCorrectCommandWithoutCapture() {
        // Arrange - captureOutput: false so git can open an editor in the terminal
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0" })), false))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.CreateTag("v1.0.0");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0" })), false))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Remote Operations

    [Fact]
    public void Fetch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "fetch", "origin" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.Fetch();

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "fetch", "origin" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PushBranch_WithSetUpstream_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "-u", "origin", "feature/test" })), false))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.PushBranch("feature/test", setUpstream: true);

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "-u", "origin", "feature/test" })), false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PushTags_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--tags" })), false))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.PushTags();

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--tags" })), false))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Global Configuration

    [Fact]
    public void GetGlobalConfigValue_WhenValueExists_ReturnsValue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "--get", "test.key" }))))
            .Returns(new GitExecutorResult(["test-value"], 0));

        // Act
        var result = _sut.GetGlobalConfigValue("test.key");

        // Assert
        result.ShouldBe("test-value");
    }

    [Fact]
    public void GetGlobalConfigValue_WhenValueDoesNotExist_ReturnsNull() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "--get", "test.key" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act
        var result = _sut.GetGlobalConfigValue("test.key");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void SetGlobalConfigValue_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "test.key", "test-value" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.SetGlobalConfigValue("test.key", "test-value");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "test.key", "test-value" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void SetGlobalConfigValue_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "test.key", "test-value" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.SetGlobalConfigValue("test.key", "test-value"));
    }

    #endregion

    #region GetRepositoryRoot

    [Fact]
    public void GetRepositoryRoot_ReturnsRepositoryRoot() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--show-toplevel" }))))
            .Returns(new GitExecutorResult(["/repo/root"], 0));

        // Act
        var result = _sut.GetRepositoryRoot();

        // Assert
        result.ShouldBe("/repo/root");
    }

    [Fact]
    public void GetRepositoryRoot_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--show-toplevel" }))))
            .Returns(new GitExecutorResult([], 128));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetRepositoryRoot());
    }

    #endregion

    #region GetLocalBranches (failure)

    [Fact]
    public void GetLocalBranches_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/heads" }))))
            .Returns(new GitExecutorResult([], 128));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetLocalBranches());
    }

    #endregion

    #region GetRemoteBranches

    [Fact]
    public void GetRemoteBranches_StripsOriginPrefix() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/remotes/origin" }))))
            .Returns(new GitExecutorResult(["origin/main", "origin/develop", "origin/HEAD"], 0));

        // Act
        var result = _sut.GetRemoteBranches();

        // Assert
        result.ShouldBe(["main", "develop"]);
    }

    [Fact]
    public void GetRemoteBranches_FilterOutHead() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/remotes/origin" }))))
            .Returns(new GitExecutorResult(["origin/HEAD", "origin/main"], 0));

        // Act
        var result = _sut.GetRemoteBranches();

        // Assert
        result.ShouldBe(["main"]);
    }

    [Fact]
    public void GetRemoteBranches_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/remotes/origin" }))))
            .Returns(new GitExecutorResult([], 128));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetRemoteBranches());
    }

    #endregion

    #region GetAllBranches

    [Fact]
    public void GetAllBranches_ReturnsSortedUnionOfLocalAndRemote() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/heads" }))))
            .Returns(new GitExecutorResult(["develop", "main"], 0));
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "for-each-ref", "--sort=refname", "--format=%(refname:short)", "refs/remotes/origin" }))))
            .Returns(new GitExecutorResult(["origin/feature/remote-only", "origin/main"], 0));

        // Act
        var result = _sut.GetAllBranches();

        // Assert - union, sorted
        result.ShouldBe(["develop", "feature/remote-only", "main"]);
    }

    #endregion

    #region IsBranchMerged

    [Fact]
    public void IsBranchMerged_WhenBranchIsMerged_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "--merged", "main" }))))
            .Returns(new GitExecutorResult(["  develop", "* main", "  feature/done"], 0));

        // Act
        var result = _sut.IsBranchMerged("feature/done", "main");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBranchMerged_WhenBranchIsNotMerged_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "branch", "--merged", "main" }))))
            .Returns(new GitExecutorResult(["  develop", "* main"], 0));

        // Act
        var result = _sut.IsBranchMerged("feature/in-progress", "main");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBranchMerged_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "branch" && a[1] == "--merged")))
            .Returns(new GitExecutorResult([], 128));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.IsBranchMerged("feature/done", "main"));
    }

    #endregion

    #region GetTags

    [Fact]
    public void GetTags_ReturnsTags() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "--list", "--sort=-version:refname" }))))
            .Returns(new GitExecutorResult(["v2.0.0", "v1.0.0"], 0));

        // Act
        var result = _sut.GetTags();

        // Assert
        result.ShouldBe(["v2.0.0", "v1.0.0"]);
    }

    [Fact]
    public void GetTags_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "--list", "--sort=-version:refname" }))))
            .Returns(new GitExecutorResult([], 128));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.GetTags());
    }

    #endregion

    #region RefExists

    [Fact]
    public void RefExists_WhenRefExists_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--verify", "--quiet", "v1.0.0" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.RefExists("v1.0.0");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void RefExists_WhenRefDoesNotExist_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rev-parse", "--verify", "--quiet", "nonexistent" }))))
            .Returns(new GitExecutorResult([], 128));

        // Act
        var result = _sut.RefExists("nonexistent");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region SetConfigValue

    [Fact]
    public void SetConfigValue_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "test.key", "test-value" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.SetConfigValue("test.key", "test-value");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "test.key", "test-value" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void SetConfigValue_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "test.key", "test-value" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.SetConfigValue("test.key", "test-value"));
    }

    #endregion

    #region GetGlobalConfigValue (empty output)

    [Fact]
    public void GetGlobalConfigValue_WhenCommandSucceedsWithNoOutput_ReturnsNull() {
        // Arrange - command succeeds but returns no output lines
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--global", "--get", "empty.key" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        var result = _sut.GetGlobalConfigValue("empty.key");

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region CheckoutBranch (failure)

    [Fact]
    public void CheckoutBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "checkout", "nonexistent" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.CheckoutBranch("nonexistent"));
    }

    #endregion

    #region DeleteLocalBranch (failure)

    [Fact]
    public void DeleteLocalBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "branch" && (a[1] == "-d" || a[1] == "-D"))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.DeleteLocalBranch("feature/test"));
    }

    #endregion

    #region DeleteRemoteBranch (failure)

    [Fact]
    public void DeleteRemoteBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--delete", "feature/test" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.DeleteRemoteBranch("feature/test"));
    }

    #endregion

    #region MergeBranch (failure)

    [Fact]
    public void MergeBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "merge")))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.MergeBranch("feature/test"));
    }

    #endregion

    #region MergeBranchSquash

    [Fact]
    public void MergeBranchSquash_ExecutesSquashMergeAndCommit() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--squash", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "commit")))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.MergeBranchSquash("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--squash", "feature/test" }))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "commit")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void MergeBranchSquash_WhenSquashFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--squash", "feature/test" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.MergeBranchSquash("feature/test"));
    }

    [Fact]
    public void MergeBranchSquash_WhenCommitFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "merge", "--squash", "feature/test" }))))
            .Returns(new GitExecutorResult([], 0));
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "commit")))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.MergeBranchSquash("feature/test"));
    }

    #endregion

    #region RebaseBranch

    [Fact]
    public void RebaseBranch_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rebase", "develop" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.RebaseBranch("develop");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rebase", "develop" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RebaseBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "rebase", "develop" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.RebaseBranch("develop"));
    }

    #endregion

    #region CreateTag (failure)

    [Fact]
    public void CreateTag_WithMessage_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0", "-m", "Release" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.CreateTag("v1.0.0", "Release"));
    }

    [Fact]
    public void CreateTag_WithoutMessage_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-a", "v1.0.0" })), false))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.CreateTag("v1.0.0"));
    }

    #endregion

    #region DeleteTag

    [Fact]
    public void DeleteTag_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-d", "v1.0.0" }))))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.DeleteTag("v1.0.0");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-d", "v1.0.0" }))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteTag_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "tag", "-d", "v1.0.0" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.DeleteTag("v1.0.0"));
    }

    #endregion

    #region Fetch (failure)

    [Fact]
    public void Fetch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "fetch", "origin" }))))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.Fetch());
    }

    #endregion

    #region PushBranch

    [Fact]
    public void PushBranch_WithoutSetUpstream_ExecutesCorrectCommand() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "feature/test" })), false))
            .Returns(new GitExecutorResult([], 0));

        // Act
        _sut.PushBranch("feature/test");

        // Assert
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "feature/test" })), false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void PushBranch_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a[0] == "push" && a[1] != "origin" || (a[0] == "push" && a.Length >= 3 && a[2] != "--tags" && a[2] != "--delete")), false))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.PushBranch("feature/test"));
    }

    #endregion

    #region PushTags (failure)

    [Fact]
    public void PushTags_WhenCommandFails_ThrowsGitException() {
        // Arrange
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "push", "origin", "--tags" })), false))
            .Returns(new GitExecutorResult([], 1));

        // Act & Assert
        Should.Throw<GitException>(() => _sut.PushTags());
    }

    #endregion

    #region Helper Methods

    private void SetupConfigGet(string key, string value) {
        A.CallTo(() => _fakeExecutor.Execute(A<string[]>.That.Matches(a => a.SequenceEqual(new[] { "config", "--get", key }))))
            .Returns(new GitExecutorResult([value], 0));
    }

    #endregion
}
