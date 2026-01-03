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
}
