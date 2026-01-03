using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Release;

/// <summary>
/// Publish a release branch to remote.
/// </summary>
public sealed class ReleasePublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings) {
        return ExecuteSafe(() => {
            var name = ReleaseService.ResolveBranchName(settings.Name);
            ReleaseService.Publish(name);
            WriteSuccess($"Published release branch '{ReleaseService.Prefix}{name}' to origin");
        });
    }
}
