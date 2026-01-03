using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Hotfix;

/// <summary>
/// Start a new hotfix branch.
/// </summary>
public sealed class HotfixStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings) {
        return ExecuteSafe(() => {
            HotfixService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started hotfix branch '{HotfixService.Prefix}{settings.Name}'");
        });
    }
}
