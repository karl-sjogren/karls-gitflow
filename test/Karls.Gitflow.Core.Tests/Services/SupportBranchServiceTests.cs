using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Core.Tests.Services;

public class SupportBranchServiceTests {
    private readonly IGitService _fakeGitService;
    private readonly SupportBranchService _sut;

    public SupportBranchServiceTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new SupportBranchService(_fakeGitService);

        // Setup default configuration
        A.CallTo(() => _fakeGitService.GetGitFlowConfiguration())
            .Returns(GitFlowConfiguration.Default);
    }

    #region Properties

    [Fact]
    public void Prefix_ReturnsSupportPrefix() {
        // Assert
        _sut.Prefix.ShouldBe("support/");
    }

    [Fact]
    public void TypeName_ReturnsSupport() {
        // Assert
        _sut.TypeName.ShouldBe("support");
    }

    #endregion

    #region GetCurrentBranchNameIfOnType

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenOnSupportBranch_ReturnsName() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("support/1.x");

        // Act
        var result = _sut.GetCurrentBranchNameIfOnType();

        // Assert
        result.ShouldBe("1.x");
    }

    [Fact]
    public void GetCurrentBranchNameIfOnType_WhenNotOnSupportBranch_ReturnsNull() {
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
    public void ResolveBranchName_WhenNameNullAndNotOnSupportBranch_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetCurrentBranchName())
            .Returns("main");

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.ResolveBranchName(null));
        ex.Message.ShouldContain("Not on a support branch");
    }

    #endregion

    #region Start - Support branches require explicit base

    [Fact]
    public void Start_WithoutBaseBranch_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        // Act & Assert - Support branches MUST have a base branch
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x"));
        ex.Message.ShouldContain("require a base branch");
    }

    [Fact]
    public void Start_WithNullBaseBranch_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x", null));
        ex.Message.ShouldContain("require a base branch");
    }

    [Fact]
    public void Start_WithEmptyBaseBranch_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x", ""));
        ex.Message.ShouldContain("require a base branch");
    }

    [Fact]
    public void Start_WithWhitespaceBaseBranch_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x", "   "));
        ex.Message.ShouldContain("require a base branch");
    }

    [Fact]
    public void Start_WithTag_CreatesFromTag() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("v1.0.0")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("v1.0.0")).Returns(true);

        // Act
        _sut.Start("1.x", "v1.0.0");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("support/1.x", "v1.0.0"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Start_WithCommitHash_CreatesFromCommit() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/legacy")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/legacy")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("abc123")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("abc123")).Returns(true);

        // Act
        _sut.Start("legacy", "abc123");

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("support/legacy", "abc123"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Start_WhenBranchAlreadyExists_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x", "v1.0.0"));
        ex.Message.ShouldContain("already exists");
    }

    [Fact]
    public void Start_WhenBaseBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("nonexistent-tag")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("nonexistent-tag")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Start("1.x", "nonexistent-tag"));
        ex.Message.ShouldContain("does not exist");
    }

    #endregion

    #region Finish - Not supported for support branches

    [Fact]
    public void Finish_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        // Act & Assert - Support branches do not have a finish operation
        var ex = Should.Throw<GitFlowException>(() => _sut.Finish("1.x"));
        ex.Message.ShouldContain("do not have a finish operation");
        ex.Message.ShouldContain("long-lived");
    }

    [Fact]
    public void Finish_WithOptions_StillThrowsGitFlowException() {
        // Arrange
        SetupValidRepositoryWithCleanWorkingTree();

        var options = new FinishOptions { Keep = true };

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Finish("1.x", options));
        ex.Message.ShouldContain("do not have a finish operation");
    }

    #endregion

    #region Publish - Support branches have simpler publish

    [Fact]
    public void Publish_WhenBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Publish("1.x"));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Publish_PushesBranchWithUpstream() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);

        // Act
        _sut.Publish("1.x");

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("support/1.x", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Publish_DoesNotCheckForExistingRemote() {
        // Arrange - Support publish doesn't check for existing remote (allows re-publish)
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(true);

        // Act - Should not throw even if remote exists
        _sut.Publish("1.x");

        // Assert
        A.CallTo(() => _fakeGitService.PushBranch("support/1.x", true))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_WhenBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Delete("1.x"));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Delete_WhenOnBranchBeingDeleted_ChecksOutMain() {
        // Arrange - Support branches default base is main
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("support/1.x");

        // Act
        _sut.Delete("1.x");

        // Assert - Should checkout main (the default base for support)
        A.CallTo(() => _fakeGitService.CheckoutBranch("main"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_DeletesLocalBranch() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("main");

        // Act
        _sut.Delete("1.x");

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("support/1.x", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_WithForceOption_ForceDeletes() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(false);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("main");

        var options = new DeleteOptions { Force = true };

        // Act
        _sut.Delete("1.x", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteLocalBranch("support/1.x", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Delete_WithRemoteOption_DeletesRemoteBranch() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("support/1.x")).Returns(true);
        A.CallTo(() => _fakeGitService.GetCurrentBranchName()).Returns("main");

        var options = new DeleteOptions { Remote = true };

        // Act
        _sut.Delete("1.x", options);

        // Assert
        A.CallTo(() => _fakeGitService.DeleteRemoteBranch("support/1.x"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region List

    [Fact]
    public void List_ReturnsOnlySupportBranches() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.GetLocalBranches())
            .Returns(["develop", "main", "support/1.x", "support/2.x", "feature/test"]);

        // Act
        var result = _sut.List();

        // Assert
        result.ShouldBe(["1.x", "2.x"]);
    }

    [Fact]
    public void List_WhenNoSupportBranches_ReturnsEmptyArray() {
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
