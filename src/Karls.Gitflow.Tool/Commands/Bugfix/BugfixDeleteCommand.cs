using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Delete a bugfix branch.
/// </summary>
public sealed class BugfixDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteDelete(BugfixService, settings);
    }
}
