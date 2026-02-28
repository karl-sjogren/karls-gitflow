using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// List all hotfix branches.
/// </summary>
public sealed class HotfixListCommand : GitFlowCommand<HotfixListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteList(HotfixService);
    }
}
