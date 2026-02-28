using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// List all feature branches.
/// </summary>
public sealed class FeatureListCommand : GitFlowCommand<FeatureListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteList(FeatureService);
    }
}
