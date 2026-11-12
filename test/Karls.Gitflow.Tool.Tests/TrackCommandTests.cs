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

        GitRepositoryFixture.InitializeBareRepository(_remoteRepoPath);

        // Create the "origin" repository (where branches are published from)
        _otherRepo = new GitRepositoryFixture();
        _otherRepo.InitializeGitFlow();
        _otherRepo.AddRemote("origin", _remoteRepoPath);
        _otherRepo.PushBranch("main");
        _otherRepo.PushBranch("develop");

        // Create the main repository that will track branches from the remote
        _repo = new GitRepositoryFixture();
        _repo.InitializeGitFlow();
        _repo.AddRemote("origin", _remoteRepoPath);
        _repo.FetchRemoteBranches();
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
        _otherRepo.CreateBranch("feature/my-feature", "develop");
        _otherRepo.CreateCommit("Feature work");
        _otherRepo.PushBranch("feature/my-feature");

        // Fetch to see the remote branch
        _repo.FetchRemoteBranches();

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
    public void FeatureTrack_WhenLocalBranchAlreadyExists_SwitchesToIt() {
        // Arrange - Publish a feature branch from the other repo
        _otherRepo.CreateBranch("feature/my-feature", "develop");
        _otherRepo.CreateCommit("Feature work");
        _otherRepo.PushBranch("feature/my-feature");

        // Create local branch in our repo first, then leave it
        _repo.FetchRemoteBranches();
        _repo.ExecuteGit("checkout -b feature/my-feature");
        _repo.ExecuteGit("checkout develop");

        // Act
        var result = _repo.ExecuteGitFlow("feature track my-feature");

        // Assert - should succeed and switch to the existing local branch
        result.Success.ShouldBeTrue();
        _repo.GetCurrentBranch().ShouldBe("feature/my-feature");
    }

    #endregion

    #region Bugfix Track

    [Fact]
    public void BugfixTrack_CreatesLocalTrackingBranch() {
        // Arrange
        _otherRepo.CreateBranch("bugfix/fix-123", "develop");
        _otherRepo.CreateCommit("Bugfix work");
        _otherRepo.PushBranch("bugfix/fix-123");

        _repo.FetchRemoteBranches();

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
        _otherRepo.CreateBranch("release/1.0.0", "develop");
        _otherRepo.CreateCommit("Release prep");
        _otherRepo.PushBranch("release/1.0.0");

        _repo.FetchRemoteBranches();

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
        _otherRepo.CreateBranch("hotfix/1.0.1", "main");
        _otherRepo.CreateCommit("Hotfix work");
        _otherRepo.PushBranch("hotfix/1.0.1");

        _repo.FetchRemoteBranches();

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
        _otherRepo.CheckoutBranch("main");
        _otherRepo.CreateTag("v1.0.0");
        _otherRepo.CreateBranch("support/1.x", "v1.0.0");
        _otherRepo.CreateCommit("Support work");
        _otherRepo.PushBranch("support/1.x");

        _repo.FetchRemoteBranches();

        // Act
        var result = _repo.ExecuteGitFlow("support track 1.x");

        // Assert
        result.Success.ShouldBeTrue();
        _repo.BranchExists("support/1.x").ShouldBeTrue();
    }

    #endregion
}
