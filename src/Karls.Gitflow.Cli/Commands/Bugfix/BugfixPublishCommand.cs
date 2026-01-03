using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Bugfix;

/// <summary>
/// Publish a bugfix branch to remote.
/// </summary>
public sealed class BugfixPublishCommand : GitFlowCommand<PublishSettings> {
    public override int Execute(CommandContext context, PublishSettings settings) {
        return ExecuteSafe(() => {
            var name = BugfixService.ResolveBranchName(settings.Name);
            BugfixService.Publish(name);
            WriteSuccess($"Published bugfix branch '{BugfixService.Prefix}{name}' to origin");
        });
    }
}
