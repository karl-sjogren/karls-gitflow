using System.IO.Abstractions;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class TrackCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;
    private readonly GitRepositoryFixture _otherRepo;
    private readonly string _remoteRepoPath;
    private readonly IFileSystem _fileSystem = new FileSystem();

    public TrackCommandTests() {
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

        // Create the "origin" repository (where branches are published from)
        _otherRepo = new GitRepositoryFixture();
        _otherRepo.ExecuteGitFlow("init -d");
        _otherRepo.ExecuteGit($"remote add origin \"{_remoteRepoPath}\"");
        _otherRepo.ExecuteGit("push -u origin main");
        _otherRepo.ExecuteGit("push -u origin develop");

        // Create the main repository that will track branches from the remote
        _repo = new GitRepositoryFixture();
        _repo.ExecuteGitFlow("init -d");
        _repo.ExecuteGit($"remote add origin \"{_remoteRepoPath}\"");
        _repo.ExecuteGit("fetch origin");
    }

    public void Dispose() {
        _repo.Dispose();
        _otherRepo.Dispose();

        // Clean up the bare remote repository
        try {
            if(_fileSystem.Directory.Exists(_remoteRepoPath)) {
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

    #region Feature Track

    [Fact]
    public void FeatureTrack_CreatesLocalTrackingBranch() {
        // Arrange - Publish a feature branch from the other repo
        _otherRepo.ExecuteGitFlow("feature start my-feature");
        _otherRepo.CreateCommit("Feature work");
        _otherRepo.ExecuteGitFlow("feature publish my-feature");

        // Fetch to see the remote branch
        _repo.ExecuteGit("fetch origin");

        // Act
        var result = _repo.ExecuteGitFlow("feature track my-feature");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("feature/my-feature");
    }

    [Fact]
    public void FeatureTrack_WhenRemoteBranchDoesNotExist_ReturnsError() {
        // Act
        var result = _repo.ExecuteGitFlow("feature track nonexistent-feature");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("does not exist");
    }

    [Fact]
    public void FeatureTrack_WhenLocalBranchAlreadyExists_ReturnsError() {
        // Arrange - Publish a feature branch from the other repo
        _otherRepo.ExecuteGitFlow("feature start my-feature");
        _otherRepo.CreateCommit("Feature work");
        _otherRepo.ExecuteGitFlow("feature publish my-feature");

        // Create local branch in our repo first
        _repo.ExecuteGit("fetch origin");
        _repo.ExecuteGit("checkout -b feature/my-feature");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("feature track my-feature");

        // Assert
        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("already exists locally");
    }

    #endregion

    #region Bugfix Track

    [Fact]
    public void BugfixTrack_CreatesLocalTrackingBranch() {
        // Arrange
        _otherRepo.ExecuteGitFlow("bugfix start fix-123");
        _otherRepo.CreateCommit("Bugfix work");
        _otherRepo.ExecuteGitFlow("bugfix publish fix-123");

        _repo.ExecuteGit("fetch origin");

        // Act
        var result = _repo.ExecuteGitFlow("bugfix track fix-123");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("bugfix/fix-123").ShouldBeTrue();
    }

    #endregion

    #region Release Track

    [Fact]
    public void ReleaseTrack_CreatesLocalTrackingBranch() {
        // Arrange
        _otherRepo.ExecuteGitFlow("release start 1.0.0");
        _otherRepo.CreateCommit("Release prep");
        _otherRepo.ExecuteGitFlow("release publish 1.0.0");

        _repo.ExecuteGit("fetch origin");

        // Act
        var result = _repo.ExecuteGitFlow("release track 1.0.0");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("release/1.0.0").ShouldBeTrue();
    }

    #endregion

    #region Hotfix Track

    [Fact]
    public void HotfixTrack_CreatesLocalTrackingBranch() {
        // Arrange
        _otherRepo.ExecuteGitFlow("hotfix start 1.0.1");
        _otherRepo.CreateCommit("Hotfix work");
        _otherRepo.ExecuteGitFlow("hotfix publish 1.0.1");

        _repo.ExecuteGit("fetch origin");

        // Act
        var result = _repo.ExecuteGitFlow("hotfix track 1.0.1");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("hotfix/1.0.1").ShouldBeTrue();
    }

    #endregion

    #region Support Track

    [Fact]
    public void SupportTrack_CreatesLocalTrackingBranch() {
        // Arrange
        _otherRepo.ExecuteGit("checkout main");
        _otherRepo.ExecuteGit("tag v1.0.0");
        _otherRepo.ExecuteGitFlow("support start 1.x v1.0.0");
        _otherRepo.CreateCommit("Support work");
        _otherRepo.ExecuteGitFlow("support publish 1.x");

        _repo.ExecuteGit("fetch origin");

        // Act
        var result = _repo.ExecuteGitFlow("support track 1.x");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeTrue();
    }

    #endregion
}
