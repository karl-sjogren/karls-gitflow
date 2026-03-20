using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class InitCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public InitCommandTests() {
        _repo = new GitRepositoryFixture();
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Init_WithDefaults_InitializesGitFlow() {
        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.IsGitFlowInitialized().ShouldBeTrue();
    }

    [Fact]
    public void Init_WithDefaults_CreatesDevelopBranch() {
        // Act
        _repo.ExecuteGitFlow("init -d");

        // Assert
        _repo.BranchExists("develop").ShouldBeTrue();
    }

    [Fact]
    public void Init_WithDefaults_SetsCorrectConfig() {
        // Act
        _repo.ExecuteGitFlow("init -d");

        // Assert
        _repo.GetConfigValue("gitflow.branch.master").ShouldBe("main");
        _repo.GetConfigValue("gitflow.branch.develop").ShouldBe("develop");
        _repo.GetConfigValue("gitflow.prefix.feature").ShouldBe("feature/");
        _repo.GetConfigValue("gitflow.prefix.bugfix").ShouldBe("bugfix/");
        _repo.GetConfigValue("gitflow.prefix.release").ShouldBe("release/");
        _repo.GetConfigValue("gitflow.prefix.hotfix").ShouldBe("hotfix/");
        _repo.GetConfigValue("gitflow.prefix.support").ShouldBe("support/");
    }

    [Fact]
    public void Init_WhenAlreadyInitialized_ReturnsError() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already initialized");
    }

    [Fact]
    public void Init_WithForce_ReinitializesGitFlow() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("init -d -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.IsGitFlowInitialized().ShouldBeTrue();
    }

    [Fact]
    public void Init_WithDefaults_EndsOnDevelopBranch() {
        // Act
        _repo.ExecuteGitFlow("init -d");

        // Assert
        _repo.GetCurrentBranch().ShouldBe("develop");
    }

    [Fact]
    public void Init_WhenDevelopBranchAlreadyExists_DoesNotRecreateIt() {
        // Arrange - Create develop branch manually
        _repo.ExecuteGit("checkout -b develop");
        _repo.CreateCommit("Existing develop commit");
        _repo.ExecuteGit("checkout main");

        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify the existing develop branch wasn't recreated
        _repo.ExecuteGit("checkout develop");
        var log = _repo.ExecuteGit("log --oneline -1");
        log.Output.ShouldContain("Existing develop commit");
    }

    // -----------------------------------------------------------------------
    // .gitflow config file: saving
    // -----------------------------------------------------------------------

    [Fact]
    public void Init_WithSaveFlag_CreatesGitFlowFile() {
        // Act
        var result = _repo.ExecuteGitFlow("init -d -s");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GitFlowFileExists().ShouldBeTrue();
    }

    [Fact]
    public void Init_WithSaveFlag_WritesCorrectSettingsToFile() {
        // Act
        _repo.ExecuteGitFlow("init -d -s --feature feat/ --tag v");

        // Assert
        var config = _repo.LoadGitFlowFile();
        config.ShouldNotBeNull();
        config!.FeaturePrefix.ShouldBe("feat/");
        config.VersionTagPrefix.ShouldBe("v");
        config.MainBranch.ShouldBe("main");
        config.DevelopBranch.ShouldBe("develop");
    }

    [Fact]
    public void Init_WithoutSaveFlag_DoesNotCreateGitFlowFile() {
        // Act
        _repo.ExecuteGitFlow("init -d");

        // Assert
        _repo.GitFlowFileExists().ShouldBeFalse();
    }

    [Fact]
    public void Init_WithSaveFlag_OutputConfirmsSave() {
        // Act
        var result = _repo.ExecuteGitFlow("init -d -s");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain(GitFlowConfigFile.FileName);
    }

    // -----------------------------------------------------------------------
    // .gitflow config file: loading
    // -----------------------------------------------------------------------

    [Fact]
    public void Init_WithDefaults_WhenGitFlowFileExists_UsesFileSettings() {
        // Arrange – put a .gitflow file with custom prefixes into the repo root
        _repo.WriteGitFlowFile(new GitFlowConfiguration {
            MainBranch = "main",
            DevelopBranch = "develop",
            FeaturePrefix = "feat/",
            BugfixPrefix = "fix/",
            ReleasePrefix = "rel/",
            HotfixPrefix = "hot/",
            SupportPrefix = "sup/",
            VersionTagPrefix = "v",
            TagMessageTemplate = "Release {version}"
        });

        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetConfigValue("gitflow.prefix.feature").ShouldBe("feat/");
        _repo.GetConfigValue("gitflow.prefix.bugfix").ShouldBe("fix/");
        _repo.GetConfigValue("gitflow.prefix.release").ShouldBe("rel/");
        _repo.GetConfigValue("gitflow.prefix.hotfix").ShouldBe("hot/");
        _repo.GetConfigValue("gitflow.prefix.support").ShouldBe("sup/");
        _repo.GetConfigValue("gitflow.prefix.versiontag").ShouldBe("v");
    }

    [Fact]
    public void Init_WithDefaults_WhenGitFlowFileExists_OutputMentionsFile() {
        // Arrange
        _repo.WriteGitFlowFile(GitFlowConfiguration.Default);

        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain(GitFlowConfigFile.FileName);
    }

    [Fact]
    public void Init_WithDefaults_WhenGitFlowFileExists_ExplicitOptionOverridesFile() {
        // Arrange – file says feature prefix is "feat/"
        _repo.WriteGitFlowFile(new GitFlowConfiguration {
            MainBranch = "main",
            DevelopBranch = "develop",
            FeaturePrefix = "feat/",
            BugfixPrefix = "bugfix/",
            ReleasePrefix = "release/",
            HotfixPrefix = "hotfix/",
            SupportPrefix = "support/"
        });

        // Act – override the feature prefix on the command line
        var result = _repo.ExecuteGitFlow("init -d --feature feature/");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetConfigValue("gitflow.prefix.feature").ShouldBe("feature/");
    }

    [Fact]
    public void Init_WithDefaults_WhenGitFlowFileDoesNotExist_UsesBuiltInDefaults() {
        // No .gitflow file – should use built-in defaults

        // Act
        var result = _repo.ExecuteGitFlow("init -d");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetConfigValue("gitflow.prefix.feature").ShouldBe("feature/");
    }
}
