using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// List gitflow configuration.
/// </summary>
public sealed class ConfigListCommand : GitFlowCommand<ConfigListCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            var config = GitService.GetGitFlowConfiguration();

            Console.MarkupLine("[blue]Gitflow Configuration:[/]");
            Console.WriteLine();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Setting")
                .AddColumn("Value");

            table.AddRow("Main branch", config.MainBranch);
            table.AddRow("Develop branch", config.DevelopBranch);
            table.AddRow("Feature prefix", config.FeaturePrefix);
            table.AddRow("Bugfix prefix", config.BugfixPrefix);
            table.AddRow("Release prefix", config.ReleasePrefix);
            table.AddRow("Hotfix prefix", config.HotfixPrefix);
            table.AddRow("Support prefix", config.SupportPrefix);
            table.AddRow("Version tag prefix", string.IsNullOrEmpty(config.VersionTagPrefix) ? "(none)" : config.VersionTagPrefix);

            Console.Write(table);
        });
    }
}

/// <summary>
/// Set a gitflow configuration value.
/// </summary>
public sealed class ConfigSetCommand : GitFlowCommand<ConfigSetCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandArgument(0, "<key>")]
        public string Key { get; set; } = string.Empty;

        [CommandArgument(1, "<value>")]
        public string Value { get; set; } = string.Empty;
    }

    private static readonly Dictionary<string, string> _configKeyMap = new(StringComparer.OrdinalIgnoreCase) {
        ["master"] = "gitflow.branch.master",
        ["main"] = "gitflow.branch.master",
        ["develop"] = "gitflow.branch.develop",
        ["feature"] = "gitflow.prefix.feature",
        ["bugfix"] = "gitflow.prefix.bugfix",
        ["release"] = "gitflow.prefix.release",
        ["hotfix"] = "gitflow.prefix.hotfix",
        ["support"] = "gitflow.prefix.support",
        ["versiontag"] = "gitflow.prefix.versiontag",
        ["tag"] = "gitflow.prefix.versiontag"
    };

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            if(!_configKeyMap.TryGetValue(settings.Key, out var configKey)) {
                var validKeys = string.Join(", ", _configKeyMap.Keys.Distinct());
                throw new Core.GitFlowException($"Unknown config key '{settings.Key}'. Valid keys: {validKeys}");
            }

            GitService.SetConfigValue(configKey, settings.Value);
            WriteSuccess($"Set {settings.Key} = {settings.Value}");
        });
    }
}
