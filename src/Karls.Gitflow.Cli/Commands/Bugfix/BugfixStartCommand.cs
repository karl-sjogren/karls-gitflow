using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Bugfix;

/// <summary>
/// Start a new bugfix branch.
/// </summary>
public sealed class BugfixStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings) {
        return ExecuteSafe(() => {
            BugfixService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started bugfix branch '{BugfixService.Prefix}{settings.Name}'");
        });
    }
}
