using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Infrastructure;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class GitServiceIntegrationTests : IDisposable {
    private readonly GitRepositoryFixture _repo;

    public GitServiceIntegrationTests() {
        _repo = new GitRepositoryFixture();
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GetGitFlowConfiguration_ReadsValuesSetByGit() {
        // Arrange - write config values using real git commands
        _repo.ExecuteGit("config gitflow.branch.master custom-main");
        _repo.ExecuteGit("config gitflow.branch.develop custom-develop");
        _repo.ExecuteGit("config gitflow.prefix.feature feat/");
        _repo.ExecuteGit("config gitflow.prefix.bugfix fix/");
        _repo.ExecuteGit("config gitflow.prefix.release rel/");
        _repo.ExecuteGit("config gitflow.prefix.hotfix hot/");
        _repo.ExecuteGit("config gitflow.prefix.support sup/");
        _repo.ExecuteGit("config gitflow.prefix.versiontag v");
        _repo.ExecuteGit("config gitflow.message.tag \"Release {version}\"");

        var gitExecutor = new GitExecutor(workingDirectory: _repo.RepositoryPath);
        var gitService = new GitService(gitExecutor);

        // Act
        var config = gitService.GetGitFlowConfiguration();

        // Assert
        config.MainBranch.ShouldBe("custom-main");
        config.DevelopBranch.ShouldBe("custom-develop");
        config.FeaturePrefix.ShouldBe("feat/");
        config.BugfixPrefix.ShouldBe("fix/");
        config.ReleasePrefix.ShouldBe("rel/");
        config.HotfixPrefix.ShouldBe("hot/");
        config.SupportPrefix.ShouldBe("sup/");
        config.VersionTagPrefix.ShouldBe("v");
        config.TagMessageTemplate.ShouldBe("Release {version}");
    }

    [Fact]
    public void GetGitFlowConfiguration_WhenNoGitflowConfig_ReturnsDefaults() {
        // Arrange - fresh repository with no gitflow config
        var gitExecutor = new GitExecutor(workingDirectory: _repo.RepositoryPath);
        var gitService = new GitService(gitExecutor);

        // Act
        var config = gitService.GetGitFlowConfiguration();

        // Assert - gitflow-specific fields should fall back to defaults
        // (GitRepositoryFixture pre-sets gitflow.message.tag, so TagMessageTemplate is excluded here)
        config.MainBranch.ShouldBe(GitFlowConfiguration.DefaultValues.MainBranch);
        config.DevelopBranch.ShouldBe(GitFlowConfiguration.DefaultValues.DevelopBranch);
        config.FeaturePrefix.ShouldBe(GitFlowConfiguration.DefaultValues.FeaturePrefix);
        config.BugfixPrefix.ShouldBe(GitFlowConfiguration.DefaultValues.BugfixPrefix);
        config.ReleasePrefix.ShouldBe(GitFlowConfiguration.DefaultValues.ReleasePrefix);
        config.HotfixPrefix.ShouldBe(GitFlowConfiguration.DefaultValues.HotfixPrefix);
        config.SupportPrefix.ShouldBe(GitFlowConfiguration.DefaultValues.SupportPrefix);
        config.VersionTagPrefix.ShouldBe(GitFlowConfiguration.DefaultValues.VersionTagPrefix);
    }
}
