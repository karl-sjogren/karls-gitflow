using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Publish a bugfix branch to remote.
/// </summary>
public sealed class BugfixPublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecutePublish(BugfixService, settings);
    }
}
