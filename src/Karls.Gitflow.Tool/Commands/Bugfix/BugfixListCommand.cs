using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// List all bugfix branches.
/// </summary>
public sealed class BugfixListCommand : GitFlowCommand<BugfixListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteList(BugfixService);
    }
}
