using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// List all release branches.
/// </summary>
public sealed class ReleaseListCommand : GitFlowCommand<ReleaseListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var releases = ReleaseService.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = ReleaseService.Prefix;

            if(releases.Length == 0) {
                WriteInfo("No release branches exist.");
                return;
            }

            foreach(var release in releases) {
                var fullName = $"{prefix}{release}";
                WriteBranch(release, currentBranch == fullName);
            }
        });
    }
}
