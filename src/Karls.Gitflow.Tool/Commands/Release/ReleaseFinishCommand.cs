using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Finish a release branch.
/// </summary>
public sealed class ReleaseFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteTagFinish(ReleaseService, settings);
    }
}
