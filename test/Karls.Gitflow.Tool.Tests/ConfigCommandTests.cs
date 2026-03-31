using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class ConfigCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public ConfigCommandTests() {
        _repo = new GitRepositoryFixture();
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ConfigList_WhenNotInitialized_ReturnsError() {
        // Act
        var result = _repo.ExecuteGitFlow("config list");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("not initialized");
    }

    [Fact]
    public void ConfigList_WhenInitialized_ShowsConfiguration() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("config list");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("Main branch");
        result.Output.ShouldContain("main");
    }

    [Fact]
    public void ConfigSet_SetsConfigValue() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("config set feature feat/");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GetConfigValue("gitflow.prefix.feature").ShouldBe("feat/");
    }

    [Fact]
    public void ConfigSet_WithInvalidKey_ReturnsError() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("config set invalidkey value");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("Unknown config key");
    }

    // -----------------------------------------------------------------------
    // config save
    // -----------------------------------------------------------------------

    [Fact]
    public void ConfigSave_WhenNotInitialized_ReturnsError() {
        // Act
        var result = _repo.ExecuteGitFlow("config save");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("not initialized");
    }

    [Fact]
    public void ConfigSave_WhenInitialized_CreatesGitFlowFile() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("config save -f");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.GitFlowFileExists().ShouldBeTrue();
    }

    [Fact]
    public void ConfigSave_WritesCurrentSettingsToFile() {
        // Arrange – init with a custom feature prefix
        _repo.ExecuteGitFlow("init -d --feature feat/");

        // Act
        _repo.ExecuteGitFlow("config save -f");

        // Assert
        var config = _repo.LoadGitFlowFile();
        config.ShouldNotBeNull();
        config!.FeaturePrefix.ShouldBe("feat/");
        config.MainBranch.ShouldBe("main");
        config.DevelopBranch.ShouldBe("develop");
    }

    [Fact]
    public void ConfigSave_WhenFileExists_WithForce_OverwritesWithoutPrompt() {
        // Arrange – create initial .gitflow file with different settings
        _repo.ExecuteGitFlow("init -d --feature feat/");
        _repo.ExecuteGitFlow("config save -f");

        // Change the feature prefix in git config
        _repo.ExecuteGit("config gitflow.prefix.feature feature/");

        // Act – save again with --force
        var result = _repo.ExecuteGitFlow("config save -f");

        // Assert
        result.Success.ShouldBeTrue();
        var config = _repo.LoadGitFlowFile();
        config!.FeaturePrefix.ShouldBe("feature/");
    }

    [Fact]
    public void ConfigSave_OutputConfirmsSave() {
        // Arrange
        _repo.ExecuteGitFlow("init -d");

        // Act
        var result = _repo.ExecuteGitFlow("config save -f");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain(Karls.Gitflow.Core.GitFlowConfigFile.FileName);
    }
}
