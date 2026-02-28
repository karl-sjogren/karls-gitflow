using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Delete a release branch.
/// </summary>
public sealed class ReleaseDeleteCommand : GitFlowCommand<DeleteSettings> {
    public override int Execute(CommandContext context, DeleteSettings settings, CancellationToken cancellationToken) {
        return ExecuteDelete(ReleaseService, settings);
    }
}
