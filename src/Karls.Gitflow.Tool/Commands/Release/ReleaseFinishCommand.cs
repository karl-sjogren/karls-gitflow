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
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                TagMessage = settings.Message,
                NoTag = settings.NoTag,
                NoBackMerge = settings.NoBackMerge
            };

            ReleaseService.Finish(name, options);

            var config = GitService.GetGitFlowConfiguration();
            var tagName = $"{config.VersionTagPrefix}{name}";

            WriteSuccess($"Finished release branch '{ReleaseService.Prefix}{name}'");
            if(!settings.NoTag) {
                WriteInfo($"Created tag '{tagName}'");
            }
        });
    }
}
