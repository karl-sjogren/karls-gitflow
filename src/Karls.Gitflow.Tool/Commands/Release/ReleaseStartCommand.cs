using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Start a new release branch.
/// </summary>
public sealed class ReleaseStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteStart(ReleaseService, settings);
    }
}
