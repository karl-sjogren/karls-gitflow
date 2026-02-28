using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Start a new bugfix branch.
/// </summary>
public sealed class BugfixStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteStart(BugfixService, settings);
    }
}
