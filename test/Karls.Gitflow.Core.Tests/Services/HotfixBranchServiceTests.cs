using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Core.Tests.Services;

public class HotfixBranchServiceTests {
    private readonly IGitService _fakeGitService;
    private readonly HotfixBranchService _sut;

    public HotfixBranchServiceTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new HotfixBranchService(_fakeGitService);

        // Setup default configuration
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default);
    }

    #region Properties

    [Fact]
    public void Prefix_ReturnsHotfixPrefix() {
        // Assert
        _sut.Prefix.ShouldBe("hotfix/");
    }

    [Fact]
    public void TypeName_ReturnsHotfix() {
        // Assert
        _sut.TypeName.ShouldBe("hotfix");
    }

    #endregion

    #region GetCurrentBranchNameIfOnType

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnHotfixBranch_ReturnsVersion() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("hotfix/1.0.1");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBe("1.0.1");
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenNotOnHotfixBranch_ReturnsNull() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("main");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region ResolveBranchName

    [Fact]
    public void ResolveBranchName_WhenNameNullAndNotOnHotfixBranch_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("main");

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.ResolveBranchName(null));
        ex.Message.ShouldContain("Not on a hotfix branch");
    }

    #endregion

    #region Start - Hotfix starts from main

    [Fact]
    public void Start_WithDefaultBase_CreatesFromMain() {
        // Arrange - Hotfix starts from main, not develop
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(true);

        // Act
        _sut.Start("1.0.1");

        // Assert - Should create from main (the default base for hotfix)
        A.CallTo(() => _fakeGitService.CreateBranch("hotfix/1.0.1", "main"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Start_WithCustomBase_CreatesFromCustomBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0")).Returns(true);

        // Act
        _sut.Start("1.0.1", "release/1.0");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("hotfix/1.0.1", "release/1.0"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Finish - Dual Merge Workflow (same as release)

    [Fact]
    public void Finish_MergesToMainAndDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.1");

        // Assert - Merge to main
        A.CallTo(() => _fakeGitService.CheckoutBranch("main"))
            .MustHaveHappened();
        A.CallTo(() => _fakeGitService.MergeBranch("hotfix/1.0.1", true))
            .MustHaveHappened();

        // Assert - Also merges to develop
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappened();
    }

    [Fact]
    public void Finish_CreatesTag() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists("1.0.1")).Returns(false);

        // Act
        _sut.Finish("1.0.1");

        // Assert - no template configured, lets git handle it
        A.CallTo(() => _fakeGitService.CreateTag("1.0.1", null))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithVersionTagPrefix_CreatesTagWithPrefix() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default with { VersionTagPrefix = "v" });

        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists("v1.0.1")).Returns(false);

        // Act
        _sut.Finish("1.0.1");

        // Assert - no template configured, lets git handle it
        A.CallTo(() => _fakeGitService.CreateTag("v1.0.1", null))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WhenTagAlreadyExists_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.TagExists("1.0.1")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Finish("1.0.1"));
        ex.Message.ShouldContain("Tag '1.0.1' already exists");
    }

    [Fact]
    public void Finish_DeletesHotfixBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.1");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("hotfix/1.0.1", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithNoTagOption_SkipsTagCreation() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);

        var options = new FinishOptions { NoTag = true };

        // Act
        _sut.Finish("1.0.1", options);

        // Assert
        A.CallTo(() => _fakeGitService.CreateTag(A<string>._, A<string?>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithNoBackMergeOption_SkipsDevelopMerge() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { NoBackMerge = true };

        // Act
        _sut.Finish("1.0.1", options);

        // Assert - Merge happens only once (to main)
        A.CallTo(() => _fakeGitService.MergeBranch(A<string>._, A<bool>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithPushOption_PushesMainDevelopAndTags() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("hotfix/1.0.1")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Push = true };

        // Act
        _sut.Finish("1.0.1", options);

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("main", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.PushBranch("develop", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.PushTags())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithKeepOption_DoesNotDeleteBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("hotfix/1.0.1")).Returns(true);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Keep = true };

        // Act
        _sut.Finish("1.0.1", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch(A<string>._, A<bool>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region List

    [Fact]
    public void List_ReturnsOnlyHotfixBranches() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.GetLocalBranches())
            .Returns(["develop", "main", "hotfix/1.0.1", "hotfix/1.0.2", "release/2.0.0"]);

        // Act
        var result = _sut.List();

        // Assert
        result.ShouldBe(["1.0.1", "1.0.2"]);
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
