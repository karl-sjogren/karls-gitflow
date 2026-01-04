using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Start a new hotfix branch.
/// </summary>
public sealed class HotfixStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            HotfixService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started hotfix branch '{HotfixService.Prefix}{settings.Name}'");
        });
    }
}
