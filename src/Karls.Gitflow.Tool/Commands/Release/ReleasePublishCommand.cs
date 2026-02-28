using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Publish a release branch to remote.
/// </summary>
public sealed class ReleasePublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecutePublish(ReleaseService, settings);
    }
}
