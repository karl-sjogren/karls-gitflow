using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Support;

/// <summary>
/// List all support branches.
/// </summary>
public sealed class SupportListCommand : GitFlowCommand<SupportListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            var supports = SupportService.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = SupportService.Prefix;

            if(supports.Length == 0) {
                WriteInfo("No support branches exist.");
                return;
            }

            foreach(var support in supports) {
                var fullName = $"{prefix}{support}";
                WriteBranch(support, currentBranch == fullName);
            }
        });
    }
}
