using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Finish a hotfix branch.
/// </summary>
public sealed class HotfixFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteTagFinish(HotfixService, settings);
    }
}
