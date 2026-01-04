using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Start a new release branch.
/// </summary>
public sealed class ReleaseStartCommand : GitFlowCommand<StartSettings> {
    public override int Execute(CommandContext context, StartSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            ReleaseService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started release branch '{ReleaseService.Prefix}{settings.Name}'");
        });
    }
}
