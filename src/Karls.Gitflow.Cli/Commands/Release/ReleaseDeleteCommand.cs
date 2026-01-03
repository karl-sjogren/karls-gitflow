using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Release;

/// <summary>
/// Delete a release branch.
/// </summary>
public sealed class ReleaseDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings) {
        return ExecuteSafe(() => {
            var name = ReleaseService.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };

            ReleaseService.Delete(name, options);
            WriteSuccess($"Deleted release branch '{ReleaseService.Prefix}{name}'");
        });
    }
}
