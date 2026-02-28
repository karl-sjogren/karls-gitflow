using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Finish a hotfix branch.
/// </summary>
public sealed class HotfixFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = HotfixService.ResolveBranchName(settings.Name);

            // Determine tag message: explicit message or null (git handles prompt)
            var tagMessage = settings.Message;

            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                TagMessage = tagMessage,
                NoTag = settings.NoTag,
                NoBackMerge = settings.NoBackMerge,
                OnProgress = CreateProgressCallback(settings.Quiet)
            };

            HotfixService.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished hotfix branch '{HotfixService.Prefix}{name}'");
            }
        });
    }
}
