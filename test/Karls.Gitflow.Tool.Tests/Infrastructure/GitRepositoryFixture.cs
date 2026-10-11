using System.Diagnostics;
using System.IO.Abstractions;
using Karls.Gitflow.Tool.Commands;
using Karls.Gitflow.Tool.Commands.Bugfix;
using Karls.Gitflow.Tool.Commands.Feature;
using Karls.Gitflow.Tool.Commands.Hotfix;
using Karls.Gitflow.Tool.Commands.Release;
using Karls.Gitflow.Tool.Commands.Support;
using Karls.Gitflow.Tool.Infrastructure;
using LibGit2Sharp;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Tests.Infrastructure;

/// <summary>
/// Provides a temporary git repository for e2e testing.
/// Creates a real git repo with initial commit and cleans up on dispose.
/// </summary>
public sealed class GitRepositoryFixture : IDisposable {
    private readonly IFileSystem _fileSystem;
    private bool _disposed;

    public string RepositoryPath { get; }

    public static void InitializeBareRepository(string repositoryPath) {
        Repository.Init(repositoryPath, isBare: true);
    }

    public GitRepositoryFixture(IFileSystem? fileSystem = null) {
        _fileSystem = fileSystem ?? new FileSystem();

        // Create a unique temp directory for the test repository
        var tempPath = _fileSystem.Path.GetTempPath();
        var repoName = $"gitflow-test-{Guid.NewGuid():N}";
        RepositoryPath = _fileSystem.Path.Combine(tempPath, repoName);

        _fileSystem.Directory.CreateDirectory(RepositoryPath);

        Repository.Init(RepositoryPath);
        using var repository = new Repository(RepositoryPath);
        repository.Config.Set("user.email", "test@example.com");
        repository.Config.Set("user.name", "Test User");
        repository.Config.Set("tag.gpgsign", false);
        repository.Config.Set("gitflow.message.tag", "{version}");

        CreateFile("README.md", "# Test Repository");
        repository.Index.Add("README.md");
        repository.Index.Write();

        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.UtcNow);
        repository.Commit("Initial commit", signature, signature);
    }

    /// <summary>
    /// Executes a git command in the repository.
    /// </summary>
    public GitCommandResult ExecuteGit(string arguments) {
        var startInfo = new ProcessStartInfo {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = RepositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new GitCommandResult(process.ExitCode, output.Trim(), error.Trim());
    }

    /// <summary>
    /// Executes the git-flow CLI command in the repository.
    /// Uses GitFlowContext for thread-local state to enable parallel test execution.
    /// </summary>
    public CliCommandResult ExecuteGitFlow(string arguments) {
        // Parse arguments - handle quoted strings properly
        var args = ParseArguments(arguments);

        // Create a custom console that writes to our StringWriter
        var outputBuilder = new System.Text.StringBuilder();
        var outputWriter = new StringWriter(outputBuilder);

        // Configure console to use our output writer
        var settings = new Spectre.Console.AnsiConsoleSettings {
            Out = new Spectre.Console.AnsiConsoleOutput(outputWriter),
            Interactive = Spectre.Console.InteractionSupport.No,
            Ansi = Spectre.Console.AnsiSupport.No
        };

        var console = Spectre.Console.AnsiConsole.Create(settings);

        // Use thread-local context for working directory and console
        GitFlowContext.WorkingDirectory = RepositoryPath;
        GitFlowContext.Console = console;

        int exitCode;
        try {
            var app = CreateCommandApp();
            exitCode = app.Run(args);
        } catch(Exception ex) {
            // If command throws, capture the error
            outputBuilder.AppendLine($"Error: {ex.Message}");
            exitCode = 1;
        } finally {
            // Reset the context
            GitFlowContext.Reset();
            outputWriter.Dispose();
        }

        return new CliCommandResult(exitCode, outputBuilder.ToString().Trim(), string.Empty);
    }

    private static string[] ParseArguments(string arguments) {
        var args = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach(var c in arguments) {
            if(c == '"') {
                inQuotes = !inQuotes;
            } else if(c == ' ' && !inQuotes) {
                if(current.Length > 0) {
                    args.Add(current.ToString());
                    current.Clear();
                }
            } else {
                current.Append(c);
            }
        }

        if(current.Length > 0) {
            args.Add(current.ToString());
        }

        return args.ToArray();
    }

    private static CommandApp CreateCommandApp() {
        var app = new CommandApp();

        app.Configure(config => {
            config.SetApplicationName("git-flow");

            config.AddCommand<InitCommand>("init");

            config.AddBranch("config", cfg => {
                cfg.AddCommand<ConfigListCommand>("list");
                cfg.AddCommand<ConfigSetCommand>("set");
                cfg.AddCommand<ConfigSaveCommand>("save");
            });

            config.AddCommand<VersionCommand>("version");
            config.AddCommand<UpdateCheckCommand>("update-check");

            config.AddBranch("feature", feature => {
                feature.AddCommand<FeatureListCommand>("list");
                feature.AddCommand<FeatureStartCommand>("start");
                feature.AddCommand<FeatureFinishCommand>("finish");
                feature.AddCommand<FeaturePublishCommand>("publish");
                feature.AddCommand<FeatureTrackCommand>("track");
                feature.AddCommand<FeatureDeleteCommand>("delete");
            });

            config.AddBranch("bugfix", bugfix => {
                bugfix.AddCommand<BugfixListCommand>("list");
                bugfix.AddCommand<BugfixStartCommand>("start");
                bugfix.AddCommand<BugfixFinishCommand>("finish");
                bugfix.AddCommand<BugfixPublishCommand>("publish");
                bugfix.AddCommand<BugfixTrackCommand>("track");
                bugfix.AddCommand<BugfixDeleteCommand>("delete");
            });

            config.AddBranch("release", release => {
                release.AddCommand<ReleaseListCommand>("list");
                release.AddCommand<ReleaseStartCommand>("start");
                release.AddCommand<ReleaseFinishCommand>("finish");
                release.AddCommand<ReleasePublishCommand>("publish");
                release.AddCommand<ReleaseTrackCommand>("track");
                release.AddCommand<ReleaseDeleteCommand>("delete");
            });

            config.AddBranch("hotfix", hotfix => {
                hotfix.AddCommand<HotfixListCommand>("list");
                hotfix.AddCommand<HotfixStartCommand>("start");
                hotfix.AddCommand<HotfixFinishCommand>("finish");
                hotfix.AddCommand<HotfixPublishCommand>("publish");
                hotfix.AddCommand<HotfixTrackCommand>("track");
                hotfix.AddCommand<HotfixDeleteCommand>("delete");
            });

            config.AddBranch("support", support => {
                support.AddCommand<SupportListCommand>("list");
                support.AddCommand<SupportStartCommand>("start");
                support.AddCommand<SupportPublishCommand>("publish");
                support.AddCommand<SupportTrackCommand>("track");
                support.AddCommand<SupportDeleteCommand>("delete");
            });

            config.AddCommand<PushCommand>("push");

            config.PropagateExceptions();
        });

        return app;
    }

    /// <summary>
    /// Creates a file in the repository.
    /// </summary>
    public void CreateFile(string relativePath, string content) {
        var fullPath = _fileSystem.Path.Combine(RepositoryPath, relativePath);
        var directory = _fileSystem.Path.GetDirectoryName(fullPath);
        if(!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory)) {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        _fileSystem.File.WriteAllText(fullPath, content);
    }

    /// <summary>
    /// Gets the current branch name.
    /// </summary>
    public string GetCurrentBranch() {
        using var repository = new Repository(RepositoryPath);
        return repository.Head.FriendlyName;
    }

    /// <summary>
    /// Gets all local branch names.
    /// </summary>
    public string[] GetBranches() {
        using var repository = new Repository(RepositoryPath);
        return repository.Branches
            .Where(branch => !branch.IsRemote)
            .Select(branch => branch.FriendlyName)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Gets all tag names.
    /// </summary>
    public string[] GetTags() {
        using var repository = new Repository(RepositoryPath);
        return repository.Tags
            .Select(tag => tag.FriendlyName)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Gets a git config value.
    /// </summary>
    public string? GetConfigValue(string key) {
        using var repository = new Repository(RepositoryPath);
        return repository.Config.Get<string>(key)?.Value;
    }

    /// <summary>
    /// Checks if a branch exists.
    /// </summary>
    public bool BranchExists(string branchName) {
        using var repository = new Repository(RepositoryPath);
        return repository.Branches.Any(branch => !branch.IsRemote && branch.FriendlyName == branchName);
    }

    /// <summary>
    /// Checks if a tag exists.
    /// </summary>
    public bool TagExists(string tagName) {
        using var repository = new Repository(RepositoryPath);
        return repository.Tags.Any(tag => tag.FriendlyName == tagName);
    }

    /// <summary>
    /// Creates a commit with a test file.
    /// </summary>
    public void CreateCommit(string message) {
        var fileName = $"file-{Guid.NewGuid():N}.txt";
        CreateFile(fileName, $"Content for {message}");

        using var repository = new Repository(RepositoryPath);
        repository.Index.Add(fileName);
        repository.Index.Write();

        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.UtcNow);
        repository.Commit(message, signature, signature);
    }

    public void InitializeGitFlow() {
        using var repository = new Repository(RepositoryPath);
        var config = Karls.Gitflow.Core.GitFlowConfiguration.Default;
        repository.Config.Set("gitflow.branch.master", config.MainBranch);
        repository.Config.Set("gitflow.branch.develop", config.DevelopBranch);
        repository.Config.Set("gitflow.prefix.feature", config.FeaturePrefix);
        repository.Config.Set("gitflow.prefix.bugfix", config.BugfixPrefix);
        repository.Config.Set("gitflow.prefix.release", config.ReleasePrefix);
        repository.Config.Set("gitflow.prefix.hotfix", config.HotfixPrefix);
        repository.Config.Set("gitflow.prefix.support", config.SupportPrefix);
        repository.Config.Set("gitflow.prefix.versiontag", config.VersionTagPrefix);
        if(!string.IsNullOrEmpty(config.TagMessageTemplate)) {
            repository.Config.Set("gitflow.message.tag", config.TagMessageTemplate);
        }

        var develop = repository.Branches.Add(config.DevelopBranch, repository.Branches[config.MainBranch].Tip);
        LibGit2Sharp.Commands.Checkout(repository, develop);
    }

    public void CheckoutBranch(string branchName) {
        using var repository = new Repository(RepositoryPath);
        var branch = repository.Branches[branchName]
            ?? throw new InvalidOperationException($"Branch '{branchName}' does not exist.");
        LibGit2Sharp.Commands.Checkout(repository, branch);
    }

    public void CreateBranch(string branchName, string? startPoint = null, bool checkout = true) {
        using var repository = new Repository(RepositoryPath);
        var startCommit = startPoint == null ? repository.Head.Tip : repository.Lookup<Commit>(startPoint)
            ?? throw new InvalidOperationException($"Start point '{startPoint}' does not exist.");
        var branch = repository.Branches.Add(branchName, startCommit);
        if(checkout) {
            LibGit2Sharp.Commands.Checkout(repository, branch);
        }
    }

    public void CreateTag(string tagName) {
        using var repository = new Repository(RepositoryPath);
        repository.Tags.Add(tagName, repository.Head.Tip);
    }

    public void AddRemote(string name, string path) {
        using var repository = new Repository(RepositoryPath);
        repository.Network.Remotes.Add(name, path);
    }

    public void PushBranch(string branchName) {
        using var repository = new Repository(RepositoryPath);
        var remote = repository.Network.Remotes["origin"]
            ?? throw new InvalidOperationException("Remote 'origin' does not exist.");
        repository.Network.Push(remote, $"refs/heads/{branchName}:refs/heads/{branchName}");
    }

    public void FetchRemoteBranches() {
        using var repository = new Repository(RepositoryPath);
        repository.Network.Fetch("origin", ["+refs/heads/*:refs/remotes/origin/*"]);
    }

    /// <summary>
    /// Checks if gitflow is initialized.
    /// </summary>
    public bool IsGitFlowInitialized() {
        using var repository = new Repository(RepositoryPath);
        return repository.Config.Get<string>("gitflow.branch.master") != null
            && repository.Config.Get<string>("gitflow.branch.develop") != null;
    }

    /// <summary>
    /// Checks whether a <c>.gitflow</c> configuration file exists in the repository root.
    /// </summary>
    public bool GitFlowFileExists() {
        return _fileSystem.File.Exists(
            _fileSystem.Path.Combine(RepositoryPath, Karls.Gitflow.Core.GitFlowConfigFile.FileName));
    }

    /// <summary>
    /// Reads and parses the <c>.gitflow</c> configuration file from the repository root.
    /// Returns <c>null</c> if the file does not exist or cannot be parsed.
    /// </summary>
    public Karls.Gitflow.Core.GitFlowConfiguration? LoadGitFlowFile() {
        var configFile = new Karls.Gitflow.Core.GitFlowConfigFile(_fileSystem);
        return configFile.Load(RepositoryPath);
    }

    /// <summary>
    /// Writes a <c>.gitflow</c> configuration file to the repository root.
    /// </summary>
    public void WriteGitFlowFile(Karls.Gitflow.Core.GitFlowConfiguration config) {
        var configFile = new Karls.Gitflow.Core.GitFlowConfigFile(_fileSystem);
        configFile.Save(RepositoryPath, config);
    }

    public void Dispose() {
        if(_disposed) {
            return;
        }

        _disposed = true;

        // Clean up the temp directory
        try {
            if(_fileSystem.Directory.Exists(RepositoryPath)) {
                // Git files can be read-only, so we need to reset attributes first
                ResetAttributes(RepositoryPath);
                _fileSystem.Directory.Delete(RepositoryPath, recursive: true);
            }
        } catch {
            // Ignore cleanup errors in tests
        }
    }

    private void ResetAttributes(string path) {
        foreach(var file in _fileSystem.Directory.GetFiles(path)) {
            _fileSystem.File.SetAttributes(file, FileAttributes.Normal);
        }

        foreach(var dir in _fileSystem.Directory.GetDirectories(path)) {
            ResetAttributes(dir);
        }
    }
}

/// <summary>
/// Result of executing a git command.
/// </summary>
public sealed record GitCommandResult(int ExitCode, string Output, string Error) {
    public bool Success => ExitCode == 0;
}

/// <summary>
/// Result of executing a CLI command.
/// </summary>
public sealed record CliCommandResult(int ExitCode, string Output, string Error) {
    public bool Success => ExitCode == 0;
}
