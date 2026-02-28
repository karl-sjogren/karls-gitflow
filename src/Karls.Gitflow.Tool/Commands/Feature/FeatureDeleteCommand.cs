using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Delete a feature branch.
/// </summary>
public sealed class FeatureDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteDelete(FeatureService, settings);
    }
}
