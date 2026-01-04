using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Start a new feature branch.
/// </summary>
public sealed class FeatureStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            FeatureService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started feature branch '{FeatureService.Prefix}{settings.Name}'");
        });
    }
}
