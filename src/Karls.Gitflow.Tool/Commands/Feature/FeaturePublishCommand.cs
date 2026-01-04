using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Publish a feature branch to remote.
/// </summary>
public sealed class FeaturePublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = FeatureService.ResolveBranchName(settings.Name);
            FeatureService.Publish(name);
            WriteSuccess($"Published feature branch '{FeatureService.Prefix}{name}' to origin");
        });
    }
}
