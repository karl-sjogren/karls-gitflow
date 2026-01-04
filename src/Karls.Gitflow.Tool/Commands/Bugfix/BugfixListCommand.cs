using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// List all bugfix branches.
/// </summary>
public sealed class BugfixListCommand : GitFlowCommand<BugfixListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var bugfixes = BugfixService.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = BugfixService.Prefix;

            if(bugfixes.Length == 0) {
                WriteInfo("No bugfix branches exist.");
                return;
            }

            foreach(var bugfix in bugfixes) {
                var fullName = $"{prefix}{bugfix}";
                WriteBranch(bugfix, currentBranch == fullName);
            }
        });
    }
}
