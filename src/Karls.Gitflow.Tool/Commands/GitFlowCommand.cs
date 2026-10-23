using Karls.Gitflow.Core;
using Karls.Gitflow.Core.Services;
using Karls.Gitflow.Tool.Infrastructure;
using System.IO.Abstractions;
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

    protected GitFlowConfigFile ConfigFile => field ??= new GitFlowConfigFile();

    private GitFlowHookRunner HookRunner => field ??= new GitFlowHookRunner(GitService, GitExecutor, Console, new FileSystem());

    /// <summary>
    /// Gets the console for output (supports thread-local override for testing).
    /// </summary>
    protected IAnsiConsole Console => GitFlowContext.EffectiveConsole;

    protected void WriteError(string message) {
        Console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
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

    protected int ExecuteHooked(
        string command,
        string? action,
        IReadOnlyDictionary<string, string?> parameters,
        IReadOnlySet<string> filterableParameters,
        Action<IReadOnlyDictionary<string, string?>> operation) {
        return ExecuteSafe(() => HookRunner.Run(command, action, parameters, filterableParameters, operation));
    }

    protected static string BranchNameParameter(IBranchService service) {
        return service.TypeName is "release" or "hotfix" or "support" ? "version" : "name";
    }

    protected static string RequiredParameter(IReadOnlyDictionary<string, string?> parameters, string name) {
        return parameters[name] ?? string.Empty;
    }

    /// <summary>
    /// Executes a list operation for the given branch service, displaying all branches.
    /// </summary>
    protected int ExecuteList(IBranchService service) {
        return ExecuteHooked(service.TypeName, "list", new Dictionary<string, string?>(), new HashSet<string>(), _ => {
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
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> {
            [nameParameter] = settings.Name,
            ["base"] = settings.BaseBranch,
            ["fetch"] = settings.Fetch.ToString()
        };

        return ExecuteHooked(service.TypeName, "start", parameters, new HashSet<string>([nameParameter, "base"]), values => {
            var name = RequiredParameter(values, nameParameter);
            var baseBranch = values["base"];
            service.Start(name, baseBranch, new StartOptions { Fetch = settings.Fetch });
            WriteSuccess($"Started {service.TypeName} branch '{service.Prefix}{name}'");
        });
    }

    /// <summary>
    /// Executes a publish operation for the given branch service.
    /// </summary>
    protected int ExecutePublish(IBranchService service, PublishSettings settings) {
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> { [nameParameter] = settings.Name };

        return ExecuteHooked(service.TypeName, "publish", parameters, new HashSet<string>([nameParameter]), values => {
            var name = service.ResolveBranchName(values[nameParameter]);
            service.Publish(name);
            WriteSuccess($"Published {service.TypeName} branch '{service.Prefix}{name}' to origin");
        });
    }

    /// <summary>
    /// Executes a delete operation for the given branch service.
    /// </summary>
    protected int ExecuteDelete(IBranchService service, DeleteSettings settings) {
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> {
            [nameParameter] = settings.Name,
            ["force"] = settings.Force.ToString(),
            ["remote"] = settings.Remote.ToString()
        };

        return ExecuteHooked(service.TypeName, "delete", parameters, new HashSet<string>([nameParameter]), values => {
            var name = service.ResolveBranchName(values[nameParameter]);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };
            service.Delete(name, options);
            WriteSuccess($"Deleted {service.TypeName} branch '{service.Prefix}{name}'");
        });
    }

    /// <summary>
    /// Executes a track operation for the given branch service.
    /// </summary>
    protected int ExecuteTrack(IBranchService service, TrackSettings settings) {
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> { [nameParameter] = settings.Name };

        return ExecuteHooked(service.TypeName, "track", parameters, new HashSet<string>([nameParameter]), values => {
            var name = RequiredParameter(values, nameParameter);
            service.Track(name);
            WriteSuccess($"Tracking {service.TypeName} branch '{service.Prefix}{name}'");
        });
    }

    /// <summary>
    /// Executes a simple finish operation (feature/bugfix pattern) for the given branch service.
    /// </summary>
    protected int ExecuteSimpleFinish(IBranchService service, SimpleFinishSettings settings) {
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> {
            [nameParameter] = settings.Name,
            ["fetch"] = settings.Fetch.ToString(),
            ["push"] = settings.Push.ToString(),
            ["keep"] = settings.Keep.ToString(),
            ["squash"] = settings.Squash.ToString(),
            ["rebase"] = settings.Rebase.ToString()
        };

        return ExecuteHooked(service.TypeName, "finish", parameters, new HashSet<string>([nameParameter]), values => {
            var name = service.ResolveBranchName(values[nameParameter]);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                KeepLocal = settings.KeepLocal,
                KeepRemote = settings.KeepRemote,
                Rebase = settings.Rebase,
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
        var nameParameter = BranchNameParameter(service);
        var parameters = new Dictionary<string, string?> {
            [nameParameter] = settings.Name,
            ["message"] = settings.Message,
            ["fetch"] = settings.Fetch.ToString(),
            ["push"] = settings.Push.ToString(),
            ["keep"] = settings.Keep.ToString(),
            ["squash"] = settings.Squash.ToString(),
            ["notag"] = settings.NoTag.ToString(),
            ["nobackmerge"] = settings.NoBackMerge.ToString()
        };

        return ExecuteHooked(service.TypeName, "finish", parameters, new HashSet<string>([nameParameter, "message"]), values => {
            var name = service.ResolveBranchName(values[nameParameter]);

            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                TagMessage = values["message"],
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
/// Settings for track commands.
/// </summary>
public class TrackSettings : BranchNameSettings {
}

/// <summary>
/// Settings for start commands.
/// </summary>
public class StartSettings : BranchNameSettings {
    [CommandArgument(1, "[base]")]
    public string? BaseBranch { get; set; }

    [CommandOption("-F|--fetch")]
    public bool Fetch { get; set; }
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
/// Settings for simple finish commands (feature/bugfix pattern) with rebase and granular keep options.
/// </summary>
public class SimpleFinishSettings : FinishSettings {
    [CommandOption("-r|--rebase")]
    public bool Rebase { get; set; }

    [CommandOption("--keeplocal")]
    public bool KeepLocal { get; set; }

    [CommandOption("--keepremote")]
    public bool KeepRemote { get; set; }
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
public abstract class BranchSimpleFinishCommand : GitFlowCommand<SimpleFinishSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, SimpleFinishSettings settings, CancellationToken cancellationToken) {
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

/// <summary>
/// Abstract base for track commands that delegate to a branch service.
/// </summary>
public abstract class BranchTrackCommand : GitFlowCommand<TrackSettings> {
    protected abstract IBranchService BranchService { get; }

    public override int Execute(CommandContext context, TrackSettings settings, CancellationToken cancellationToken) {
        return ExecuteTrack(BranchService, settings);
    }
}
