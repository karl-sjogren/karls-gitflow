using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Delete a support branch.
/// </summary>
public sealed class SupportDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = SupportService.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };

            SupportService.Delete(name, options);
            WriteSuccess($"Deleted support branch '{SupportService.Prefix}{name}'");
        });
    }
}
