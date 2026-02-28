using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Finish a bugfix branch.
/// </summary>
public sealed class BugfixFinishCommand : GitFlowCommand<FinishSettings> {
    public override int Execute(CommandContext context, FinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSimpleFinish(BugfixService, settings);
    }
}
