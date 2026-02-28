using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Publish a hotfix branch to remote.
/// </summary>
public sealed class HotfixPublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings, CancellationToken cancellationToken) {
        return ExecutePublish(HotfixService, settings);
    }
}
