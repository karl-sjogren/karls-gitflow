using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Finish a release branch.
/// </summary>
public sealed class ReleaseFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = ReleaseService.ResolveBranchName(settings.Name);

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

            ReleaseService.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished release branch '{ReleaseService.Prefix}{name}'");
            }
        });
    }
}
