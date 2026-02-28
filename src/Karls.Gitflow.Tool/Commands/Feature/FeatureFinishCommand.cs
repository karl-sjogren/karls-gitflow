using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Finish a feature branch.
/// </summary>
public sealed class FeatureFinishCommand : GitFlowCommand<FinishSettings> {
    public override int Execute(CommandContext context, FinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSimpleFinish(FeatureService, settings);
    }
}
