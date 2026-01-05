using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Publish a release branch to remote.
/// </summary>
public sealed class ReleasePublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = ReleaseService.ResolveBranchName(settings.Name);
            var messages = ReleaseService.Publish(name);
            WriteSuccess($"Published release branch '{ReleaseService.Prefix}{name}' to origin");
            WriteServerMessages(messages);
        });
    }
}
