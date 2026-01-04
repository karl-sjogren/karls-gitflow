using System.IO.Abstractions;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class PushCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo;
    private readonly string _remoteRepoPath;
    private readonly IFileSystem _fileSystem = new FileSystem();

    public PushCommandTests() {
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

    [Fact]
    public void Push_PushesMainBranch() {
        // Arrange - Make a change on main
        _repo.ExecuteGit("checkout main");
        _repo.CreateCommit("Main commit");
        var localCommit = _repo.ExecuteGit("rev-parse main").Output;

        // Act
        var result = _repo.ExecuteGitFlow("push");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify main was pushed
        _repo.ExecuteGit("fetch origin");
        var remoteCommit = _repo.ExecuteGit("rev-parse origin/main").Output;
        remoteCommit.ShouldBe(localCommit);
    }

    [Fact]
    public void Push_PushesDevelopBranch() {
        // Arrange - Make a change on develop
        _repo.ExecuteGit("checkout develop");
        _repo.CreateCommit("Develop commit");
        var localCommit = _repo.ExecuteGit("rev-parse develop").Output;

        // Act
        var result = _repo.ExecuteGitFlow("push");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify develop was pushed
        _repo.ExecuteGit("fetch origin");
        var remoteCommit = _repo.ExecuteGit("rev-parse origin/develop").Output;
        remoteCommit.ShouldBe(localCommit);
    }

    [Fact]
    public void Push_PushesTags() {
        // Arrange - Create a tag
        _repo.ExecuteGit("checkout main");
        _repo.ExecuteGit("tag -a v1.0.0 -m \"Version 1.0.0\"");

        // Act
        var result = _repo.ExecuteGitFlow("push");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify tag was pushed by checking remote tags
        _repo.ExecuteGit("fetch origin --tags");
        var tags = _repo.ExecuteGit("tag -l");
        tags.Output.ShouldContain("v1.0.0");
    }

    [Fact]
    public void Push_AfterReleaseFinish_PushesAll() {
        // Arrange - Create and finish a release
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release prep");
        _repo.ExecuteGitFlow("release finish 1.0.0 -m \"Release 1.0.0\"");

        // Get local state
        var mainCommit = _repo.ExecuteGit("rev-parse main").Output;
        var developCommit = _repo.ExecuteGit("rev-parse develop").Output;

        // Act
        var result = _repo.ExecuteGitFlow("push");

        // Assert
        result.Success.ShouldBeTrue();

        // Verify everything was pushed
        _repo.ExecuteGit("fetch origin --tags");

        var remoteMain = _repo.ExecuteGit("rev-parse origin/main").Output;
        var remoteDevelop = _repo.ExecuteGit("rev-parse origin/develop").Output;

        remoteMain.ShouldBe(mainCommit);
        remoteDevelop.ShouldBe(developCommit);

        // Tag should be pushed
        var tags = _repo.GetTags();
        tags.ShouldContain("1.0.0");
    }

    [Fact]
    public void Push_ShowsProgressMessages() {
        // Arrange
        _repo.ExecuteGit("checkout develop");
        _repo.CreateCommit("Test commit");

        // Act
        var result = _repo.ExecuteGitFlow("push");

        // Assert
        result.Success.ShouldBeTrue();
        result.Output.ShouldContain("Pushing");
        result.Output.ShouldContain("main");
        result.Output.ShouldContain("develop");
    }
}
