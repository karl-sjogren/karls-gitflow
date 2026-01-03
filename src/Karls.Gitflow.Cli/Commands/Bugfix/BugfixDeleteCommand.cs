using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Bugfix;

/// <summary>
/// Delete a bugfix branch.
/// </summary>
public sealed class BugfixDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings) {
        return ExecuteSafe(() => {
            var name = BugfixService.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };

            BugfixService.Delete(name, options);
            WriteSuccess($"Deleted bugfix branch '{BugfixService.Prefix}{name}'");
        });
    }
}
