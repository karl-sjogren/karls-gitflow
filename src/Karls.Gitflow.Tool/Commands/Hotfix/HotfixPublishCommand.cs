using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Publish a hotfix branch to remote.
/// </summary>
public sealed class HotfixPublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings) {
        return ExecuteSafe(() => {
            var name = HotfixService.ResolveBranchName(settings.Name);
            HotfixService.Publish(name);
            WriteSuccess($"Published hotfix branch '{HotfixService.Prefix}{name}' to origin");
        });
    }
}
