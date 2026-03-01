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
    protected IGitExecutor GitExecutor { get; } = new GitExecutor();

    protected IGitService GitService => field ??= new GitService(GitExecutor);

    protected FeatureBranchService FeatureService => field ??= new(GitService);

    protected BugfixBranchService BugfixService => field ??= new(GitService);

    protected ReleaseBranchService ReleaseService => field ??= new(GitService);

    protected HotfixBranchService HotfixService => field ??= new(GitService);

    protected SupportBranchService SupportService => field ??= new(GitService);

    protected GitFlowInitializer Initializer => field ??= new(GitService);

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
    /// Executes a list operation for the given branch service, displaying all branches.
    /// </summary>
    protected int ExecuteList(IBranchService service) {
        return ExecuteSafe(() => {
            var branches = service.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = service.Prefix;

            if(branches.Length == 0) {
                WriteInfo($"No {service.TypeName} branches exist.");
                return;
            }

            foreach(var branch in branches) {
                WriteBranch(branch, currentBranch == $"{prefix}{branch}");
            }
        });
    }

    /// <summary>
    /// Executes a start operation for the given branch service.
    /// </summary>
    protected int ExecuteStart(IBranchService service, StartSettings settings) {
        return ExecuteSafe(() => {
            service.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started {service.TypeName} branch '{service.Prefix}{settings.Name}'");
        });
    }

    /// <summary>
    /// Executes a publish operation for the given branch service.
    /// </summary>
    protected int ExecutePublish(IBranchService service, PublishSettings settings) {
        return ExecuteSafe(() => {
            var name = service.ResolveBranchName(settings.Name);
            service.Publish(name);
            WriteSuccess($"Published {service.TypeName} branch '{service.Prefix}{name}' to origin");
        });
    }

    /// <summary>
    /// Executes a delete operation for the given branch service.
    /// </summary>
    protected int ExecuteDelete(IBranchService service, DeleteSettings settings) {
        return ExecuteSafe(() => {
            var name = service.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };
            service.Delete(name, options);
            WriteSuccess($"Deleted {service.TypeName} branch '{service.Prefix}{name}'");
        });
    }

    /// <summary>
    /// Executes a simple finish operation (feature/bugfix pattern) for the given branch service.
    /// </summary>
    protected int ExecuteSimpleFinish(IBranchService service, FinishSettings settings) {
        return ExecuteSafe(() => {
            var name = service.ResolveBranchName(settings.Name);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                OnProgress = CreateProgressCallback(settings.Quiet)
            };

            service.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished {service.TypeName} branch '{service.Prefix}{name}'");
            }
        });
    }

    /// <summary>
    /// Executes a tag-based finish operation (release/hotfix pattern) for the given branch service.
    /// </summary>
    protected int ExecuteTagFinish(IBranchService service, TagFinishSettings settings) {
        return ExecuteSafe(() => {
            var name = service.ResolveBranchName(settings.Name);

            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                TagMessage = settings.Message,
                NoTag = settings.NoTag,
                NoBackMerge = settings.NoBackMerge,
                OnProgress = CreateProgressCallback(settings.Quiet)
            };

            service.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished {service.TypeName} branch '{service.Prefix}{name}'");
            }
        });
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

/// <summary>
/// Settings for list commands (no arguments).
/// </summary>
public class ListSettings : CommandSettings {
}

/// <summary>
/// Abstract base for list commands that delegate to a branch service.
/// </summary>
public abstract class BranchListCommand : GitFlowCommand<ListSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, ListSettings settings, CancellationToken cancellationToken) {
        return ExecuteList(BranchService);
    }
}

/// <summary>
/// Abstract base for start commands that delegate to a branch service.
/// </summary>
public abstract class BranchStartCommand : GitFlowCommand<StartSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteStart(BranchService, settings);
    }
}

/// <summary>
/// Abstract base for publish commands that delegate to a branch service.
/// </summary>
public abstract class BranchPublishCommand : GitFlowCommand<PublishSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecutePublish(BranchService, settings);
    }
}

/// <summary>
/// Abstract base for delete commands that delegate to a branch service.
/// </summary>
public abstract class BranchDeleteCommand : GitFlowCommand<DeleteSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteDelete(BranchService, settings);
    }
}

/// <summary>
/// Abstract base for simple finish commands (feature/bugfix pattern) that delegate to a branch service.
/// </summary>
public abstract class BranchSimpleFinishCommand : GitFlowCommand<FinishSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, FinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSimpleFinish(BranchService, settings);
    }
}

/// <summary>
/// Abstract base for tag-based finish commands (release/hotfix pattern) that delegate to a branch service.
/// </summary>
public abstract class BranchTagFinishCommand : GitFlowCommand<TagFinishSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, TagFinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteTagFinish(BranchService, settings);
    }
}
