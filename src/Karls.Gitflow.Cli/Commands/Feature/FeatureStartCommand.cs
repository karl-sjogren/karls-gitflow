using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Feature;

/// <summary>
/// Start a new feature branch.
/// </summary>
public sealed class FeatureStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings) {
        return ExecuteSafe(() => {
            FeatureService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started feature branch '{FeatureService.Prefix}{settings.Name}'");
        });
    }
}
