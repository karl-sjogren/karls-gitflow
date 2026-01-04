using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Push main, develop, and tags to remote.
/// </summary>
public sealed class PushCommand : GitFlowCommand<PushCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var config = GitService.GetGitFlowConfiguration();

            WriteInfo("Pushing main branch...");
            GitService.PushBranch(config.MainBranch);
            WriteSuccess($"Pushed '{config.MainBranch}'");

            WriteInfo("Pushing develop branch...");
            GitService.PushBranch(config.DevelopBranch);
            WriteSuccess($"Pushed '{config.DevelopBranch}'");

            WriteInfo("Pushing tags...");
            GitService.PushTags();
            WriteSuccess("Pushed tags");
        });
    }
}
