using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Core.Tests.Services;

/// <summary>
/// Tests for BugfixBranchService. Since it inherits from BranchServiceBase and uses
/// FinishSimpleMerge (same as FeatureBranchService), we only test the bugfix-specific
/// behavior here. See FeatureBranchServiceTests for comprehensive base class tests.
/// </summary>
public class BugfixBranchServiceTests {
    private readonly IGitService _fakeGitService;
    private readonly BugfixBranchService _sut;

    public BugfixBranchServiceTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new BugfixBranchService(_fakeGitService);

        // Setup default configuration
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default);
    }

    #region Properties

    [Fact]
    public void Prefix_ReturnsBugfixPrefix() {
        // Assert
        _sut.Prefix.ShouldBe("bugfix/");
    }

    [Fact]
    public void TypeName_ReturnsBugfix() {
        // Assert
        _sut.TypeName.ShouldBe("bugfix");
    }

    #endregion

    #region GetCurrentBranchNameIfOnType

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnBugfixBranch_ReturnsName() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("bugfix/fix-login");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBe("fix-login");
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenNotOnBugfixBranch_ReturnsNull() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("develop");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region Start

    [Fact]
    public void Start_WithDefaultBase_CreatesFromDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("bugfix/fix-login")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("bugfix/fix-login")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(true);

        // Act
        _sut.Start("fix-login");

        // Assert - Bugfix starts from develop, same as feature
        A.CallTo(() => _fakeGitService.CreateBranch("bugfix/fix-login", "develop"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Finish

    [Fact]
    public void Finish_MergesIntoDevelopAndDeletesBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("bugfix/fix-login")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("bugfix/fix-login")).Returns(false);

        // Act
        _sut.Finish("fix-login");

        // Assert - Same behavior as feature finish
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.MergeBranch("bugfix/fix-login", true))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("bugfix/fix-login", true))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region List

    [Fact]
    public void List_ReturnsOnlyBugfixBranches() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.GetLocalBranches())
            .Returns(["develop", "main", "bugfix/fix-login", "bugfix/fix-crash", "feature/new-feature"]);

        // Act
        var result = _sut.List();

        // Assert
        result.ShouldBe(["fix-login", "fix-crash"]);
    }

    #endregion

    #region Helper Methods

    private void SetupValidRepository() {
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(true);
    }

    private void SetupValidRepositoryWithCleanWorkingTree() {
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsWorkingTreeClean()).Returns(true);
    }

    #endregion
}
