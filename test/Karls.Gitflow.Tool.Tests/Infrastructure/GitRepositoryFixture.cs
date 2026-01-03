using System.Diagnostics;
using System.IO.Abstractions;
using Karls.Gitflow.Tool.Commands;
using Karls.Gitflow.Tool.Commands.Bugfix;
using Karls.Gitflow.Tool.Commands.Feature;
using Karls.Gitflow.Tool.Commands.Hotfix;
using Karls.Gitflow.Tool.Commands.Release;
using Karls.Gitflow.Tool.Commands.Support;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Tests.Infrastructure;

/// <summary>
/// Provides a temporary git repository for e2e testing.
/// Creates a real git repo with initial commit and cleans up on dispose.
/// </summary>
public sealed class GitRepositoryFixture : IDisposable {
    private readonly IFileSystem _fileSystem;
    private readonly string _originalDirectory;
    private bool _disposed;

    public string RepositoryPath { get; }

    public GitRepositoryFixture(IFileSystem? fileSystem = null) {
        _fileSystem = fileSystem ?? new FileSystem();
        _originalDirectory = Environment.CurrentDirectory;

        // Create a unique temp directory for the test repository
        var tempPath = _fileSystem.Path.GetTempPath();
        var repoName = $"gitflow-test-{Guid.NewGuid():N}";
        RepositoryPath = _fileSystem.Path.Combine(tempPath, repoName);

        _fileSystem.Directory.CreateDirectory(RepositoryPath);

        // Initialize git repository
        ExecuteGit("init");

        // Configure git user for commits (required for commits to work)
        ExecuteGit("config user.email \"test@example.com\"");
        ExecuteGit("config user.name \"Test User\"");

        // Create initial commit on main branch
        ExecuteGit("checkout -b main");
        CreateFile("README.md", "# Test Repository");
        ExecuteGit("add .");
        ExecuteGit("commit -m \"Initial commit\"");
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
    /// Changes to the repository directory, runs the command, then restores the original directory.
    /// </summary>
    public CliCommandResult ExecuteGitFlow(string arguments) {
        // Parse arguments - handle quoted strings properly
        var args = ParseArguments(arguments);

        // Change to repository directory
        var previousDir = Environment.CurrentDirectory;
        Environment.CurrentDirectory = RepositoryPath;

        // Create a custom console that writes to our StringWriter
        var outputBuilder = new System.Text.StringBuilder();
        var outputWriter = new StringWriter(outputBuilder);

        // Configure AnsiConsole to use our output writer
        var settings = new Spectre.Console.AnsiConsoleSettings {
            Out = new Spectre.Console.AnsiConsoleOutput(outputWriter),
            Interactive = Spectre.Console.InteractionSupport.No,
            Ansi = Spectre.Console.AnsiSupport.No
        };

        var console = Spectre.Console.AnsiConsole.Create(settings);

        // Replace the static console (this is a bit hacky but necessary for testing)
        var originalConsole = Spectre.Console.AnsiConsole.Console;
        Spectre.Console.AnsiConsole.Console = console;

        int exitCode;
        try {
            var app = CreateCommandApp();
            exitCode = app.Run(args);
        } catch(Exception ex) {
            // If command throws, capture the error
            outputBuilder.AppendLine($"Error: {ex.Message}");
            exitCode = 1;
        } finally {
            Spectre.Console.AnsiConsole.Console = originalConsole;

            // Restore previous directory if it still exists
            try {
                Environment.CurrentDirectory = previousDir;
            } catch(DirectoryNotFoundException) {
                // Previous directory was cleaned up, use temp
                Environment.CurrentDirectory = _fileSystem.Path.GetTempPath();
            }

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
            });

            config.AddCommand<VersionCommand>("version");

            config.AddBranch("feature", feature => {
                feature.AddCommand<FeatureListCommand>("list");
                feature.AddCommand<FeatureStartCommand>("start");
                feature.AddCommand<FeatureFinishCommand>("finish");
                feature.AddCommand<FeaturePublishCommand>("publish");
                feature.AddCommand<FeatureDeleteCommand>("delete");
            });

            config.AddBranch("bugfix", bugfix => {
                bugfix.AddCommand<BugfixListCommand>("list");
                bugfix.AddCommand<BugfixStartCommand>("start");
                bugfix.AddCommand<BugfixFinishCommand>("finish");
                bugfix.AddCommand<BugfixPublishCommand>("publish");
                bugfix.AddCommand<BugfixDeleteCommand>("delete");
            });

            config.AddBranch("release", release => {
                release.AddCommand<ReleaseListCommand>("list");
                release.AddCommand<ReleaseStartCommand>("start");
                release.AddCommand<ReleaseFinishCommand>("finish");
                release.AddCommand<ReleasePublishCommand>("publish");
                release.AddCommand<ReleaseDeleteCommand>("delete");
            });

            config.AddBranch("hotfix", hotfix => {
                hotfix.AddCommand<HotfixListCommand>("list");
                hotfix.AddCommand<HotfixStartCommand>("start");
                hotfix.AddCommand<HotfixFinishCommand>("finish");
                hotfix.AddCommand<HotfixPublishCommand>("publish");
                hotfix.AddCommand<HotfixDeleteCommand>("delete");
            });

            config.AddBranch("support", support => {
                support.AddCommand<SupportListCommand>("list");
                support.AddCommand<SupportStartCommand>("start");
                support.AddCommand<SupportDeleteCommand>("delete");
            });

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
        var result = ExecuteGit("rev-parse --abbrev-ref HEAD");
        return result.Output;
    }

    /// <summary>
    /// Gets all local branch names.
    /// </summary>
    public string[] GetBranches() {
        var result = ExecuteGit("branch --format=%(refname:short)");
        return result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets all tag names.
    /// </summary>
    public string[] GetTags() {
        var result = ExecuteGit("tag --list");
        if(string.IsNullOrWhiteSpace(result.Output)) {
            return [];
        }

        return result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets a git config value.
    /// </summary>
    public string? GetConfigValue(string key) {
        var result = ExecuteGit($"config --get {key}");
        return result.ExitCode == 0 ? result.Output : null;
    }

    /// <summary>
    /// Checks if a branch exists.
    /// </summary>
    public bool BranchExists(string branchName) {
        var result = ExecuteGit($"show-ref --verify --quiet refs/heads/{branchName}");
        return result.ExitCode == 0;
    }

    /// <summary>
    /// Checks if a tag exists.
    /// </summary>
    public bool TagExists(string tagName) {
        var result = ExecuteGit($"show-ref --verify --quiet refs/tags/{tagName}");
        return result.ExitCode == 0;
    }

    /// <summary>
    /// Creates a commit with a test file.
    /// </summary>
    public void CreateCommit(string message) {
        var fileName = $"file-{Guid.NewGuid():N}.txt";
        CreateFile(fileName, $"Content for {message}");
        ExecuteGit("add .");
        ExecuteGit($"commit -m \"{message}\"");
    }

    /// <summary>
    /// Checks if gitflow is initialized.
    /// </summary>
    public bool IsGitFlowInitialized() {
        return GetConfigValue("gitflow.branch.master") != null
            && GetConfigValue("gitflow.branch.develop") != null;
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
