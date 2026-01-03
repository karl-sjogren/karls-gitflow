using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Hotfix;

/// <summary>
/// Delete a hotfix branch.
/// </summary>
public sealed class HotfixDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings) {
        return ExecuteSafe(() => {
            var name = HotfixService.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };

            HotfixService.Delete(name, options);
            WriteSuccess($"Deleted hotfix branch '{HotfixService.Prefix}{name}'");
        });
    }
}
