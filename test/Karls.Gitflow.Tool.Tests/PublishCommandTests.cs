using System.IO.Abstractions;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class PublishCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;
    private readonly string _remoteRepoPath;
    private readonly IFileSystem _fileSystem = new FileSystem();

    public PublishCommandTests() {
        // Create a bare "remote" repository
        var tempPath = _fileSystem.Path.GetTempPath();
        _remoteRepoPath = _fileSystem.Path.Combine(tempPath, $"gitflow-remote-{Guid.NewGuid():N}");
        _fileSystem.Directory.CreateDirectory(_remoteRepoPath);

        // Initialize as bare repository
        var bareInit = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
            FileName = "git",
            Arguments = "init --bare",
            WorkingDirectory = _remoteRepoPath,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        bareInit?.WaitForExit();

        // Create the main repository
        _repo = new GitRepositoryFixture();
        _repo.ExecuteGitFlow("init -d");

        // Add the remote
        _repo.ExecuteGit($"remote add origin \"{_remoteRepoPath}\"");

        // Push main and develop to establish remote branches
        _repo.ExecuteGit("push -u origin main");
        _repo.ExecuteGit("push -u origin develop");
    }

    public void Dispose() {
        _repo.Dispose();

        // Clean up the bare remote repository
        try {
            if(_fileSystem.Directory.Exists(_remoteRepoPath)) {
                // Git files can be read-only
                foreach(var file in _fileSystem.Directory.GetFiles(_remoteRepoPath, "*", SearchOption.AllDirectories)) {
                    _fileSystem.File.SetAttributes(file, FileAttributes.Normal);
                }

                _fileSystem.Directory.Delete(_remoteRepoPath, recursive: true);
            }
        } catch {
            // Ignore cleanup errors
        }

        GC.SuppressFinalize(this);
    }

    #region Feature Publish

    [Fact]
    public void FeaturePublish_PushesFeatureBranchToRemote() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");

        // Act
        var result = _repo.ExecuteGitFlow("feature publish my-feature");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify remote branch exists
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/feature/my-feature");
    }

    [Fact]
    public void FeaturePublish_WhenOnFeatureBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");

        // Act - Don't specify the name
        var result = _repo.ExecuteGitFlow("feature publish");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/feature/my-feature");
    }

    [Fact]
    public void FeaturePublish_WhenAlreadyPublished_ReturnsError() {
        // Arrange
        _repo.ExecuteGitFlow("feature start my-feature");
        _repo.CreateCommit("Feature work");
        _repo.ExecuteGitFlow("feature publish my-feature");

        // Act
        var result = _repo.ExecuteGitFlow("feature publish my-feature");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already exists on remote");
    }

    #endregion

    #region Bugfix Publish

    [Fact]
    public void BugfixPublish_PushesBugfixBranchToRemote() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix publish fix-123");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/bugfix/fix-123");
    }

    [Fact]
    public void BugfixPublish_WhenOnBugfixBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("bugfix start fix-123");
        _repo.CreateCommit("Bugfix work");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix publish");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/bugfix/fix-123");
    }

    #endregion

    #region Release Publish

    [Fact]
    public void ReleasePublish_PushesReleaseBranchToRemote() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release prep");

        // Act
        var result = _repo.ExecuteGitFlow("release publish 1.0.0");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/release/1.0.0");
    }

    [Fact]
    public void ReleasePublish_WhenOnReleaseBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release prep");

        // Act
        var result = _repo.ExecuteGitFlow("release publish");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/release/1.0.0");
    }

    #endregion

    #region Hotfix Publish

    [Fact]
    public void HotfixPublish_PushesHotfixBranchToRemote() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix publish 1.0.1");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/hotfix/1.0.1");
    }

    [Fact]
    public void HotfixPublish_WhenOnHotfixBranch_AutoDetectsName() {
        // Arrange
        _repo.ExecuteGitFlow("hotfix start 1.0.1");
        _repo.CreateCommit("Hotfix work");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix publish");

        // Assert
        result.Success.ShouldBeTrue();
        var remoteBranches = _repo.ExecuteGit("branch -r");
        remoteBranches.Output.ShouldContain("origin/hotfix/1.0.1");
    }

    #endregion
}
