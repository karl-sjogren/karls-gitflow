using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Delete a feature branch.
/// </summary>
public sealed class FeatureDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings) {
        return ExecuteSafe(() => {
            var name = FeatureService.ResolveBranchName(settings.Name);
            var options = new DeleteOptions {
                Force = settings.Force,
                Remote = settings.Remote
            };

            FeatureService.Delete(name, options);
            WriteSuccess($"Deleted feature branch '{FeatureService.Prefix}{name}'");
        });
    }
}
