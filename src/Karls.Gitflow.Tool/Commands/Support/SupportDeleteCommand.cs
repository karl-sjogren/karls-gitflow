using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Delete a support branch.
/// </summary>
public sealed class SupportDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteDelete(SupportService, settings);
    }
}
