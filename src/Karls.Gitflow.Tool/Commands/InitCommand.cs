using Karls.Gitflow.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Initialize gitflow in a repository.
/// </summary>
public sealed class InitCommand : GitFlowCommand<InitCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandOption("-d|--defaults")]
        public bool UseDefaults { get; set; }

        [CommandOption("-f|--force")]
        public bool Force { get; set; }

        [CommandOption("--main <BRANCH>")]
        public string? MainBranch { get; set; }

        [CommandOption("--develop <BRANCH>")]
        public string? DevelopBranch { get; set; }

        [CommandOption("--feature <PREFIX>")]
        public string? FeaturePrefix { get; set; }

        [CommandOption("--bugfix <PREFIX>")]
        public string? BugfixPrefix { get; set; }

        [CommandOption("--release <PREFIX>")]
        public string? ReleasePrefix { get; set; }

        [CommandOption("--hotfix <PREFIX>")]
        public string? HotfixPrefix { get; set; }

        [CommandOption("--support <PREFIX>")]
        public string? SupportPrefix { get; set; }

        [CommandOption("--tag <PREFIX>")]
        public string? VersionTagPrefix { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            if(!GitService.IsGitRepository()) {
                throw new GitFlowException("Not a git repository.");
            }

            if(Initializer.IsInitialized && !settings.Force) {
                throw new GitFlowException("Gitflow is already initialized. Use --force to reinitialize.");
            }

            GitFlowConfiguration config;

            if(settings.UseDefaults) {
                config = CreateConfigFromSettings(settings, GitFlowConfiguration.Default);
            } else {
                config = PromptForConfiguration(settings);
            }

            Initializer.Initialize(config, settings.Force);

            WriteSuccess("Gitflow initialized successfully!");
            Console.WriteLine();
            WriteInfo("Configuration:");
            Console.MarkupLine($"  Main branch:      [yellow]{config.MainBranch}[/]");
            Console.MarkupLine($"  Develop branch:   [yellow]{config.DevelopBranch}[/]");
            Console.MarkupLine($"  Feature prefix:   [yellow]{config.FeaturePrefix}[/]");
            Console.MarkupLine($"  Bugfix prefix:    [yellow]{config.BugfixPrefix}[/]");
            Console.MarkupLine($"  Release prefix:   [yellow]{config.ReleasePrefix}[/]");
            Console.MarkupLine($"  Hotfix prefix:    [yellow]{config.HotfixPrefix}[/]");
            Console.MarkupLine($"  Support prefix:   [yellow]{config.SupportPrefix}[/]");
            Console.MarkupLine($"  Version tag:      [yellow]{(string.IsNullOrEmpty(config.VersionTagPrefix) ? "(none)" : config.VersionTagPrefix)}[/]");
        });
    }

    private GitFlowConfiguration PromptForConfiguration(Settings settings) {
        var defaults = GitFlowConfiguration.Default;
        var localBranches = GitService.GetLocalBranches();

        Console.MarkupLine("[blue]Initializing gitflow...[/]");
        Console.WriteLine();

        // Main branch
        var mainBranch = settings.MainBranch ?? PromptBranch(
            "Which branch should be used for production releases?",
            defaults.MainBranch,
            localBranches);

        // Develop branch
        var developBranch = settings.DevelopBranch ?? PromptBranch(
            "Which branch should be used for integration?",
            defaults.DevelopBranch,
            localBranches);

        // Prefixes
        var featurePrefix = settings.FeaturePrefix ??
            Console.Prompt(new TextPrompt<string>("Feature branch prefix?")
                .DefaultValue(defaults.FeaturePrefix));

        var bugfixPrefix = settings.BugfixPrefix ??
            Console.Prompt(new TextPrompt<string>("Bugfix branch prefix?")
                .DefaultValue(defaults.BugfixPrefix));

        var releasePrefix = settings.ReleasePrefix ??
            Console.Prompt(new TextPrompt<string>("Release branch prefix?")
                .DefaultValue(defaults.ReleasePrefix));

        var hotfixPrefix = settings.HotfixPrefix ??
            Console.Prompt(new TextPrompt<string>("Hotfix branch prefix?")
                .DefaultValue(defaults.HotfixPrefix));

        var supportPrefix = settings.SupportPrefix ??
            Console.Prompt(new TextPrompt<string>("Support branch prefix?")
                .DefaultValue(defaults.SupportPrefix));

        var versionTagPrefix = settings.VersionTagPrefix ??
            Console.Prompt(new TextPrompt<string>("Version tag prefix?")
                .DefaultValue(defaults.VersionTagPrefix)
                .AllowEmpty());

        return new GitFlowConfiguration {
            MainBranch = mainBranch,
            DevelopBranch = developBranch,
            FeaturePrefix = featurePrefix,
            BugfixPrefix = bugfixPrefix,
            ReleasePrefix = releasePrefix,
            HotfixPrefix = hotfixPrefix,
            SupportPrefix = supportPrefix,
            VersionTagPrefix = versionTagPrefix
        };
    }

    private string PromptBranch(string prompt, string defaultValue, string[] existingBranches) {
        var choices = existingBranches.ToList();
        if(!choices.Contains(defaultValue)) {
            choices.Insert(0, defaultValue);
        }

        return Console.Prompt(
            new SelectionPrompt<string>()
                .Title(prompt)
                .AddChoices(choices)
                .HighlightStyle(Style.Parse("yellow")));
    }

    private static GitFlowConfiguration CreateConfigFromSettings(Settings settings, GitFlowConfiguration defaults) {
        return new GitFlowConfiguration {
            MainBranch = settings.MainBranch ?? defaults.MainBranch,
            DevelopBranch = settings.DevelopBranch ?? defaults.DevelopBranch,
            FeaturePrefix = settings.FeaturePrefix ?? defaults.FeaturePrefix,
            BugfixPrefix = settings.BugfixPrefix ?? defaults.BugfixPrefix,
            ReleasePrefix = settings.ReleasePrefix ?? defaults.ReleasePrefix,
            HotfixPrefix = settings.HotfixPrefix ?? defaults.HotfixPrefix,
            SupportPrefix = settings.SupportPrefix ?? defaults.SupportPrefix,
            VersionTagPrefix = settings.VersionTagPrefix ?? defaults.VersionTagPrefix
        };
    }
}
