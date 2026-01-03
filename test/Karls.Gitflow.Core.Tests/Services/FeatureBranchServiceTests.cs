using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Core.Tests.Services;

public class FeatureBranchServiceTests {
    private readonly IGitService _fakeGitService;
    private readonly FeatureBranchService _sut;

    public FeatureBranchServiceTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new FeatureBranchService(_fakeGitService);

        // Setup default configuration
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default);
    }

    #region Properties

    [Fact]
    public void Prefix_ReturnsFeaturePrefix() {
        // Assert
        _sut.Prefix.ShouldBe("feature/");
    }

    [Fact]
    public void TypeName_ReturnsFeature() {
        // Assert
        _sut.TypeName.ShouldBe("feature");
    }

    #endregion

    #region GetCurrentBranchNameIfOnType

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnFeatureBranch_ReturnsName() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("feature/my-feature");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBe("my-feature");
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenNotOnFeatureBranch_ReturnsNull() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("develop");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnDifferentBranchType_ReturnsNull() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("release/1.0.0");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region ResolveBranchName

    [Fact]
    public void ResolveBranchName_WhenNameProvided_ReturnsName() {
        // Act
        var result = _sut.ResolveBranchName("explicit-name");

        // Assert
        result.ShouldBe("explicit-name");
    }

    [Fact]
    public void ResolveBranchName_WhenNameNullAndOnFeatureBranch_ReturnsDetectedName() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("feature/auto-detected");

        // Act
        var result = _sut.ResolveBranchName(null);

        // Assert
        result.ShouldBe("auto-detected");
    }

    [Fact]
    public void ResolveBranchName_WhenNameEmptyAndOnFeatureBranch_ReturnsDetectedName() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("feature/auto-detected");

        // Act
        var result = _sut.ResolveBranchName("");

        // Assert
        result.ShouldBe("auto-detected");
    }

    [Fact]
    public void ResolveBranchName_WhenNameNullAndNotOnFeatureBranch_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("develop");

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.ResolveBranchName(null));
        ex.Message.ShouldContain("Not on a feature branch");
    }

    #endregion

    #region List

    [Fact]
    public void List_WhenNotGitRepository_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.List());
        ex.Message.ShouldContain("Not a git repository");
    }

    [Fact]
    public void List_WhenGitFlowNotInitialized_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.List());
        ex.Message.ShouldContain("not initialized");
    }

    [Fact]
    public void List_ReturnsOnlyFeatureBranches() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.GetLocalBranches())
            .Returns(["develop", "main", "feature/one", "feature/two", "release/1.0.0"]);

        // Act
        var result = _sut.List();

        // Assert
        result.ShouldBe(["one", "two"]);
    }

    [Fact]
    public void List_WhenNoFeatureBranches_ReturnsEmptyArray() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.GetLocalBranches())
            .Returns(["develop", "main"]);

        // Act
        var result = _sut.List();

        // Assert
        result.ShouldBeEmpty();
    }

    #endregion

    #region Start

    [Fact]
    public void Start_WhenWorkingTreeDirty_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsWorkingTreeClean()).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("my-feature"));
        ex.Message.ShouldContain("uncommitted changes");
    }

    [Fact]
    public void Start_WhenBranchAlreadyExists_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsWorkingTreeClean()).Returns(true);
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("my-feature"));
        ex.Message.ShouldContain("already exists");
    }

    [Fact]
    public void Start_WhenBranchExistsOnRemote_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsWorkingTreeClean()).Returns(true);
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("my-feature"));
        ex.Message.ShouldContain("already exists on remote");
    }

    [Fact]
    public void Start_WhenBaseBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsWorkingTreeClean()).Returns(true);
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("develop")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("my-feature"));
        ex.Message.ShouldContain("Base branch");
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Start_WithDefaultBase_CreatesFromDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(true);
        A.CallTo(() => _fakeGitService.CreateBranch(A<string>._, A<string>._))
            .DoesNothing();

        // Act
        _sut.Start("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("feature/my-feature", "develop"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Start_WithCustomBase_CreatesFromCustomBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("custom-base")).Returns(true);
        A.CallTo(() => _fakeGitService.CreateBranch(A<string>._, A<string>._))
            .DoesNothing();

        // Act
        _sut.Start("my-feature", "custom-base");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("feature/my-feature", "custom-base"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Finish

    [Fact]
    public void Finish_WhenBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Finish("my-feature"));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Finish_MergesIntoDevelopAndDeletesBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        // Act
        _sut.Finish("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.MergeBranch("feature/my-feature", true))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("feature/my-feature", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WhenRemoteBranchExists_DeletesRemote() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);

        // Act
        _sut.Finish("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch("feature/my-feature"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithKeepOption_DoesNotDeleteBranch() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);

        var options = new FinishOptions { Keep = true };

        // Act
        _sut.Finish("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch(A<string>._, A<bool>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Finish_WithFetchOption_FetchesFirst() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        var options = new FinishOptions { Fetch = true };

        // Act
        _sut.Finish("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.Fetch())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithPushOption_PushesDevelop() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        var options = new FinishOptions { Push = true };

        // Act
        _sut.Finish("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("develop", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Finish_WithSquashOption_SquashesMerge() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        var options = new FinishOptions { Squash = true };

        // Act
        _sut.Finish("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.MergeBranchSquash("feature/my-feature"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.MergeBranch(A<string>._, A<bool>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region Publish

    [Fact]
    public void Publish_WhenBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Publish("my-feature"));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Publish_WhenRemoteAlreadyExists_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Publish("my-feature"));
        ex.Message.ShouldContain("already exists on remote");
    }

    [Fact]
    public void Publish_PushesBranchWithUpstream() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        // Act
        _sut.Publish("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("feature/my-feature", true))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_WhenBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Delete("my-feature"));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Delete_WhenOnBranchBeingDeleted_ChecksOutDevelop() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("feature/my-feature");

        // Act
        _sut.Delete("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.CheckoutBranch("develop"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_DeletesLocalBranch() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("develop");

        // Act
        _sut.Delete("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("feature/my-feature", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_WithForceOption_ForceDeletes() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("develop");

        var options = new DeleteOptions { Force = true };

        // Act
        _sut.Delete("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("feature/my-feature", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_WithRemoteOption_DeletesRemoteBranch() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("develop");

        var options = new DeleteOptions { Remote = true };

        // Act
        _sut.Delete("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch("feature/my-feature"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_WithoutRemoteOption_DoesNotDeleteRemoteBranch() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("develop");

        // Act
        _sut.Delete("my-feature");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Delete_WhenOnlyRemoteExists_DeletesOnlyRemote() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("feature/my-feature")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("feature/my-feature")).Returns(true);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("develop");

        var options = new DeleteOptions { Remote = true };

        // Act
        _sut.Delete("my-feature", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch(A<string>._, A<bool>._))
            .MustNotHaveHappened();
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch("feature/my-feature"))
            .MustHaveHappenedOnceExactly();
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
