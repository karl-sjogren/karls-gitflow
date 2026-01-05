namespace Karls.Gitflow.Core.Tests;

public class GitFlowInitializerTests {
    private readonly IGitService _fakeGitService;
    private readonly GitFlowInitializer _sut;

    public GitFlowInitializerTests() {
        _fakeGitService = A.Fake<IGitService>();
        _sut = new GitFlowInitializer(_fakeGitService);
    }

    #region IsInitialized

    [Fact]
    public void IsInitialized_WhenGitFlowInitialized_ReturnsTrue() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(true);

        // Act & Assert
        _sut.IsInitialized.ShouldBeTrue();
    }

    [Fact]
    public void IsInitialized_WhenGitFlowNotInitialized_ReturnsFalse() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);

        // Act & Assert
        _sut.IsInitialized.ShouldBeFalse();
    }

    #endregion

    #region Initialize

    [Fact]
    public void Initialize_WhenNotGitRepository_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("Not a git repository");
    }

    [Fact]
    public void Initialize_WhenAlreadyInitializedWithoutForce_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(true);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("already initialized");
    }

    [Fact]
    public void Initialize_WhenAlreadyInitializedWithForce_Succeeds() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(true);

        // Act
        _sut.Initialize(GitFlowConfiguration.Default, force: true);

        // Assert - Should have written config
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.branch.master", "main"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Initialize_WithInvalidConfig_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);

        var invalidConfig = new GitFlowConfiguration {
            MainBranch = "", // Invalid
            DevelopBranch = "develop",
            FeaturePrefix = "feature/",
            BugfixPrefix = "bugfix/",
            ReleasePrefix = "release/",
            HotfixPrefix = "hotfix/",
            SupportPrefix = "support/"
        };

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(invalidConfig));
        ex.Message.ShouldContain("Invalid");
    }

    [Fact]
    public void Initialize_WhenMainBranchDoesNotExist_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.GetLocalBranches()).Returns(["some-branch"]);
        A.CallTo(() => _fakeGitService.RemoteBranchExists(A<string>._)).Returns(false);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("does not exist");
    }

    [Fact]
    public void Initialize_WhenMainBranchExistsOnlyOnRemote_ChecksOutRemoteBranch() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("main")).Returns(true);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(false);

        // Act
        _sut.Initialize(GitFlowConfiguration.Default);

        // Assert - Should have checked out the remote branch
        A.CallTo(() => _fakeGitService.CheckoutBranch("main")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Initialize_WhenMainBranchExistsButDifferentName_SuggestsExistingBranch() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("master")).Returns(true);
        A.CallTo(() => _fakeGitService.GetLocalBranches()).Returns(["master", "develop"]);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("master");
    }

    [Fact]
    public void Initialize_WhenAlternativeBranchExistsOnlyOnRemote_SuggestsRemoteBranch() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists(A<string>._)).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("master")).Returns(true);
        A.CallTo(() => _fakeGitService.GetLocalBranches()).Returns(["some-branch"]);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("master");
    }

    [Fact]
    public void Initialize_WhenEmptyRepository_ThrowsGitFlowException() {
        // Arrange
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.RemoteBranchExists("main")).Returns(false);
        A.CallTo(() => _fakeGitService.GetLocalBranches()).Returns([]);

        // Act & Assert
        var ex = Should.Throw<GitFlowException>(() => _sut.Initialize(GitFlowConfiguration.Default));
        ex.Message.ShouldContain("no branches");
    }

    [Fact]
    public void Initialize_CreatesDevelopBranchIfNotExists() {
        // Arrange
        SetupValidRepository();
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(false);

        // Act
        _sut.Initialize(GitFlowConfiguration.Default);

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("develop", "main"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Initialize_DoesNotCreateDevelopBranchIfExists() {
        // Arrange
        SetupValidRepository();

        // Act
        _sut.Initialize(GitFlowConfiguration.Default);

        // Assert
        A.CallTo(() => _fakeGitService.CreateBranch("develop", A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Initialize_WritesAllConfigurationKeys() {
        // Arrange
        SetupValidRepository();

        var config = new GitFlowConfiguration {
            MainBranch = "main",
            DevelopBranch = "develop",
            FeaturePrefix = "feat/",
            BugfixPrefix = "fix/",
            ReleasePrefix = "rel/",
            HotfixPrefix = "hot/",
            SupportPrefix = "sup/",
            VersionTagPrefix = "v"
        };

        // Act
        _sut.Initialize(config);

        // Assert
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.branch.master", "main")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.branch.develop", "develop")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.feature", "feat/")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.bugfix", "fix/")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.release", "rel/")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.hotfix", "hot/")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.support", "sup/")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.versiontag", "v")).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region InitializeWithDefaults

    [Fact]
    public void InitializeWithDefaults_UsesDefaultConfiguration() {
        // Arrange
        SetupValidRepository();

        // Act
        _sut.InitializeWithDefaults();

        // Assert
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.branch.master", "main")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetConfigValue("gitflow.prefix.feature", "feature/")).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Helper Methods

    private void SetupValidRepository() {
        A.CallTo(() => _fakeGitService.IsGitRepository()).Returns(true);
        A.CallTo(() => _fakeGitService.IsGitFlowInitialized()).Returns(false);
        A.CallTo(() => _fakeGitService.LocalBranchExists("main")).Returns(true);
        A.CallTo(() => _fakeGitService.LocalBranchExists("develop")).Returns(true);
        A.CallTo(() => _fakeGitService.GetLocalBranches()).Returns(["main", "develop"]);
    }

    #endregion
}
