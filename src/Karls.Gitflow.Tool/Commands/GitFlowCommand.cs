using System.Diagnostics;
using System.IO.Abstractions;
using Karls.Gitflow.Core;
using Karls.Gitflow.Core.Services;
using Karls.Gitflow.Tool.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Base class for all gitflow commands with common functionality.
/// </summary>
public abstract class GitFlowCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings {
    private readonly IFileSystem _fileSystem = new FileSystem();
    protected IGitExecutor GitExecutor { get; } = new GitExecutor();
    protected IGitService GitService => new GitService(GitExecutor);

    protected FeatureBranchService FeatureService => new(GitService);
    protected BugfixBranchService BugfixService => new(GitService);
    protected ReleaseBranchService ReleaseService => new(GitService);
    protected HotfixBranchService HotfixService => new(GitService);
    protected SupportBranchService SupportService => new(GitService);
    protected GitFlowInitializer Initializer => new(GitService);

    /// <summary>
    /// Gets the console for output (supports thread-local override for testing).
    /// </summary>
    protected IAnsiConsole Console => GitFlowContext.EffectiveConsole;

    protected void WriteError(string message) {
        Console.MarkupLine($"[red]Error:[/] {message}");
    }

    protected void WriteSuccess(string message) {
        Console.MarkupLine($"[green]{message}[/]");
    }

    protected void WriteInfo(string message) {
        Console.MarkupLine($"[blue]{message}[/]");
    }

    /// <summary>
    /// Creates a progress callback for reporting operation status.
    /// Returns null if quiet mode is enabled.
    /// </summary>
    protected Action<string>? CreateProgressCallback(bool quiet) {
        return quiet ? null : WriteInfo;
    }

    /// <summary>
    /// Writes server messages (e.g., PR links, security warnings) to the console.
    /// </summary>
    protected void WriteServerMessages(string[] messages) {
        foreach(var message in messages) {
            if(!string.IsNullOrWhiteSpace(message)) {
                Console.MarkupLine($"[grey]{Markup.Escape(message)}[/]");
            }
        }
    }

    protected void WriteBranch(string branchName, bool isCurrent = false) {
        if(isCurrent) {
            Console.MarkupLine($"[green]* {branchName}[/]");
        } else {
            Console.MarkupLine($"  {branchName}");
        }
    }

    protected int ExecuteSafe(Action action) {
        try {
            action();
            return 0;
        } catch(GitFlowException ex) {
            WriteError(ex.Message);
            return 1;
        } catch(GitException ex) {
            WriteError(ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Prompts the user to enter a tag message using their configured editor.
    /// </summary>
    /// <param name="tagName">The tag name for the template.</param>
    /// <returns>The message entered by the user, or null if cancelled/empty.</returns>
    /// <exception cref="GitFlowException">Thrown if no editor is configured or editor fails.</exception>
    protected string? PromptForTagMessage(string tagName) {
        var editor = GetEditor();
        if(string.IsNullOrEmpty(editor)) {
            throw new GitFlowException(
                "No editor configured. Set core.editor in git config, or use -m to provide a message.");
        }

        var tempFile = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), $"TAG_EDITMSG_{Guid.NewGuid():N}");
        try {
            // Write template to temp file
            var template = $"""

                # Enter a tag message for '{tagName}'.
                # Lines starting with '#' will be ignored.
                # An empty message aborts the tagging.
                """;
            _fileSystem.File.WriteAllText(tempFile, template);

            // Open editor
            var process = StartEditor(editor, tempFile);
            if(process == null) {
                throw new GitFlowException($"Failed to start editor: {editor}");
            }

            process.WaitForExit();

            if(process.ExitCode != 0) {
                throw new GitFlowException($"Editor exited with code {process.ExitCode}");
            }

            // Read and parse the result
            var content = _fileSystem.File.ReadAllText(tempFile);
            var message = ParseTagMessage(content);

            if(string.IsNullOrWhiteSpace(message)) {
                throw new GitFlowException("Aborting due to empty tag message.");
            }

            return message;
        } finally {
            if(_fileSystem.File.Exists(tempFile)) {
                _fileSystem.File.Delete(tempFile);
            }
        }
    }

    private string? GetEditor() {
        // Check git config at each level: local, global, system
        var scopes = new[] { "--local", "--global", "--system" };
        foreach(var scope in scopes) {
            var result = GitExecutor.Execute($"config {scope} --get core.editor");
            if(result.ExitCode == 0 && result.Output.Length > 0 && !string.IsNullOrEmpty(result.Output[0])) {
                return result.Output[0];
            }
        }

        // Fall back to environment variables
        var editor = Environment.GetEnvironmentVariable("GIT_EDITOR");
        if(!string.IsNullOrEmpty(editor)) {
            return editor;
        }

        editor = Environment.GetEnvironmentVariable("VISUAL");
        if(!string.IsNullOrEmpty(editor)) {
            return editor;
        }

        editor = Environment.GetEnvironmentVariable("EDITOR");
        if(!string.IsNullOrEmpty(editor)) {
            return editor;
        }

        return null;
    }

    private static Process? StartEditor(string editor, string filePath) {
        // Handle editors with arguments (e.g., "code --wait")
        var parts = editor.Split(' ', 2);
        var exe = parts[0];
        var args = parts.Length > 1 ? $"{parts[1]} \"{filePath}\"" : $"\"{filePath}\"";

        var startInfo = new ProcessStartInfo {
            FileName = exe,
            Arguments = args,
            UseShellExecute = false
        };

        return Process.Start(startInfo);
    }

    private static string ParseTagMessage(string content) {
        var lines = content.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !line.StartsWith('#'));

        return string.Join("\n", lines).Trim();
    }
}

/// <summary>
/// Settings for branch commands that require a name.
/// </summary>
public class BranchNameSettings : CommandSettings {
    [CommandArgument(0, "<name>")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Settings for branch commands where the name is optional (auto-detected from current branch).
/// </summary>
public class OptionalBranchNameSettings : CommandSettings {
    [CommandArgument(0, "[name]")]
    public string? Name { get; set; }
}

/// <summary>
/// Settings for start commands.
/// </summary>
public class StartSettings : BranchNameSettings {
    [CommandArgument(1, "[base]")]
    public string? BaseBranch { get; set; }
}

/// <summary>
/// Settings for finish commands (name optional - auto-detected from current branch).
/// </summary>
public class FinishSettings : OptionalBranchNameSettings {
    [CommandOption("-F|--fetch")]
    public bool Fetch { get; set; }

    [CommandOption("-p|--push")]
    public bool Push { get; set; }

    [CommandOption("-k|--keep")]
    public bool Keep { get; set; }

    [CommandOption("-S|--squash")]
    public bool Squash { get; set; }

    [CommandOption("-q|--quiet")]
    public bool Quiet { get; set; }
}

/// <summary>
/// Settings for finish commands with tagging support.
/// </summary>
public class TagFinishSettings : FinishSettings {
    [CommandOption("-m|--message <MESSAGE>")]
    public string? Message { get; set; }

    [CommandOption("-n|--notag")]
    public bool NoTag { get; set; }

    [CommandOption("-b|--nobackmerge")]
    public bool NoBackMerge { get; set; }
}

/// <summary>
/// Settings for publish commands (name optional - auto-detected from current branch).
/// </summary>
public class PublishSettings : OptionalBranchNameSettings {
}

/// <summary>
/// Settings for delete commands (name optional - auto-detected from current branch).
/// </summary>
public class DeleteSettings : OptionalBranchNameSettings {
    [CommandOption("-f|--force")]
    public bool Force { get; set; }

    [CommandOption("-r|--remote")]
    public bool Remote { get; set; }
}
