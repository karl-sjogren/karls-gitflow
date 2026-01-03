using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// List all feature branches.
/// </summary>
public sealed class FeatureListCommand : GitFlowCommand<FeatureListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            var features = FeatureService.List();
            var currentBranch = GitService.GetCurrentBranchName();
            var prefix = FeatureService.Prefix;

            if(features.Length == 0) {
                WriteInfo("No feature branches exist.");
                return;
            }

            foreach(var feature in features) {
                var fullName = $"{prefix}{feature}";
                WriteBranch(feature, currentBranch == fullName);
            }
        });
    }
}
