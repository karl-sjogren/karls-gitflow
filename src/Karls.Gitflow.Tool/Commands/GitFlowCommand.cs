using Karls.Gitflow.Tool.Infrastructure;
using Karls.Gitflow.Core;
using Karls.Gitflow.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Base class for all gitflow commands with common functionality.
/// </summary>
public abstract class GitFlowCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings {
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
