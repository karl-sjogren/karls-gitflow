using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Finish a bugfix branch.
/// </summary>
public sealed class BugfixFinishCommand : GitFlowCommand<FinishSettings> {
    public override int Execute(CommandContext context, FinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = BugfixService.ResolveBranchName(settings.Name);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                OnProgress = CreateProgressCallback(settings.Quiet)
            };

            BugfixService.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished bugfix branch '{BugfixService.Prefix}{name}'");
            }
        });
    }
}
