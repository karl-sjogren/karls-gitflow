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
}
