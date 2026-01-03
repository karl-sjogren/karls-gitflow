using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Core.Tests.Services;

public class ReleaseBranchServiceTests {
    private readonly IGitService _fakeGitService;
    private readonly ReleaseBranchService _sut;

    public ReleaseBranchServiceTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new ReleaseBranchService(_fakeGitService);

        // Setup default configuration
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default);
    }

    #region Properties

    [Fact]
    public void Prefix_ReturnsReleasePrefix() {
        // Assert
        _sut.Prefix.ShouldBe("release/");
    }

    [Fact]
    public void TypeName_ReturnsRelease() {
        // Assert
        _sut.TypeName.ShouldBe("release");
    }

    #endregion

    #region GetCurrentBranchNameIfOnType

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnReleaseBranch_ReturnsVersion() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("release/1.0.0");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBe("1.0.0");
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenNotOnReleaseBranch_ReturnsNull() {
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
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(true);

        // Act
        _sut.Start("1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("release/1.0.0", "develop"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Finish - Dual Merge Workflow

    [Fact]
    public void Finish_MergesToMainAndDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert - Step 1: Merge to main
        A.CallTo(() => _fakeGitService.CheckoutBranch("main"))
            .MustHaveHappened();
        A.CallTo(() => _fakeGitService.MergeBranch("release/1.0.0", true))
            .MustHaveHappened();

        // Assert - Step 3: Merge to develop
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappened();
    }

    [Fact]
    public void Finish_CreatesTag() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists("1.0.0")).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.CreateTag("1.0.0", "Release 1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithVersionTagPrefix_CreatesTagWithPrefix() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default with { VersionTagPrefix = "v" });

        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists("v1.0.0")).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.CreateTag("v1.0.0", "Release 1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WhenTagAlreadyExists_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.TagExists("1.0.0")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Finish("1.0.0"));
        ex.Message.ShouldContain("Tag '1.0.0' already exists");
    }

    [Fact]
    public void Finish_DeletesReleaseBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("release/1.0.0", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WhenRemoteBranchExists_DeletesRemote() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch("release/1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithKeepOption_DoesNotDeleteBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Keep = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch(A<string>._, A<bool>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithNoTagOption_SkipsTagCreation() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);

        var options = new FinishOptions { NoTag = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.CreateTag(A<string>._, A<string?>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithNoBackMergeOption_SkipsDevelopMerge() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { NoBackMerge = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert - Should checkout main but not develop for merge
        A.CallTo(() => _fakeGitService.CheckoutBranch("main"))
            .MustHaveHappenedOnceExactly();

        // Verify merge happens only once (to main)
        A.CallTo(() => _fakeGitService.MergeBranch(A<string>._, A<bool>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithCustomTagMessage_UsesMessage() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { TagMessage = "Custom release message" };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.CreateTag("1.0.0", "Custom release message"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithFetchOption_FetchesFirst() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Fetch = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.Fetch())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithPushOption_PushesMainDevelopAndTags() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Push = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("main", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.PushBranch("develop", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.PushTags())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithPushAndNoTagOptions_DoesNotPushTags() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);

        var options = new FinishOptions { Push = true, NoTag = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.PushTags())
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithPushAndNoBackMergeOptions_DoesNotPushDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Push = true, NoBackMerge = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("main", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.PushBranch("develop", A<bool>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithSquashOption_SquashesMergeToMain() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        var options = new FinishOptions { Squash = true };

        // Act
        _sut.Finish("1.0.0", options);

        // Assert
        A.CallTo(() => _fakeGitService.MergeBranchSquash("release/1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_ReturnsToDevelopBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("release/1.0.0")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("release/1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.TagExists(A<string>._)).Returns(false);

        // Act
        _sut.Finish("1.0.0");

        // Assert - The last checkout should be to develop
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappened();
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
