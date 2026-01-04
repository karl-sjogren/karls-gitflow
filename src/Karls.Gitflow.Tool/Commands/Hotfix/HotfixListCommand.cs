using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// List all hotfix branches.
/// </summary>
public sealed class HotfixListCommand : GitFlowCommand<HotfixListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var hotfixes = HotfixService.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = HotfixService.Prefix;

            if(hotfixes.Length == 0) {
                WriteInfo("No hotfix branches exist.");
                return;
            }

            foreach(var hotfix in hotfixes) {
                var fullName = $"{prefix}{hotfix}";
                WriteBranch(hotfix, currentBranch == fullName);
            }
        });
    }
}
