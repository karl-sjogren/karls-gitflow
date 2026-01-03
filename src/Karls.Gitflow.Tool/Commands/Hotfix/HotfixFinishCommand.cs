using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Finish a hotfix branch.
/// </summary>
public sealed class HotfixFinishCommand : GitFlowCommand<TagFinishSettings> {
    public override int Execute(CommandContext context, TagFinishSettings settings) {
        return ExecuteSafe(() => {
            var name = HotfixService.ResolveBranchName(settings.Name);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                TagMessage = settings.Message,
                NoTag = settings.NoTag,
                NoBackMerge = settings.NoBackMerge
            };

            HotfixService.Finish(name, options);

            var config = GitService.GetGitFlowConfiguration();
            var tagName = $"{config.VersionTagPrefix}{name}";

            WriteSuccess($"Finished hotfix branch '{HotfixService.Prefix}{name}'");
            if(!settings.NoTag) {
                WriteInfo($"Created tag '{tagName}'");
            }
        });
    }
}
