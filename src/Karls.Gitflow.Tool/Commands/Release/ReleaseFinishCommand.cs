using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Finish a release branch.
/// </summary>
public sealed class ReleaseFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings) {
        return ExecuteSafe(() => {
            var name = ReleaseService.ResolveBranchName(settings.Name);
            var config = GitService.GetGitFlowConfiguration();
            var tagName = $"{config.VersionTagPrefix}{name}";

            // Determine tag message: explicit > template > editor prompt
            var tagMessage = settings.Message;
            if(!settings.NoTag && string.IsNullOrEmpty(tagMessage)) {
                if(!string.IsNullOrEmpty(config.TagMessageTemplate)) {
                    // Template will be applied in the service layer
                    tagMessage = null;
                } else {
                    // No message and no template - prompt with editor
                    tagMessage = PromptForTagMessage(tagName);
                }
            }

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
