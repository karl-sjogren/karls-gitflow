using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// List all release branches.
/// </summary>
public sealed class ReleaseListCommand : GitFlowCommand<ReleaseListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteList(ReleaseService);
    }
}
