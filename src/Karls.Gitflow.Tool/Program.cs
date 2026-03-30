using System.Reflection;
using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Commands;
using Karls.Gitflow.Tool.Commands.Bugfix;
using Karls.Gitflow.Tool.Commands.Feature;
using Karls.Gitflow.Tool.Commands.Hotfix;
using Karls.Gitflow.Tool.Commands.Release;
using Karls.Gitflow.Tool.Commands.Support;
using Karls.Gitflow.Tool.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

#region Check for updates

try {
    var gitExecutor = new GitExecutor();
    var gitService = new GitService(gitExecutor);
    var nugetClient = new NuGetApiClient();
    var promptService = new UpdatePromptService();
    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

    var updateChecker = new UpdateChecker(gitService, nugetClient, promptService, currentVersion);
    var shouldExit = await updateChecker.CheckForUpdatesAsync();

    nugetClient.Dispose();

    if(shouldExit) {
        return 0; // User chose to update, exit gracefully
    }
} catch {
    // Silent failure - never interrupt workflow
}

#endregion

var app = new CommandApp();

app.Configure(config => {
    config.SetApplicationName("git-flow");
    config.SetApplicationVersion(Assembly.GetExecutingAssembly().GetName().Version is { } v ? $"{v.Major}.{v.Minor}.{v.Build}" : "0.0.0");

    // Init command
    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize a new git repo with support for the branching model.");

    // Config command
    config.AddBranch("config", config => {
        config.SetDescription("Manage gitflow configuration.");
        config.AddCommand<ConfigListCommand>("list")
            .WithDescription("List gitflow configuration.");
        config.AddCommand<ConfigSetCommand>("set")
            .WithDescription("Set a gitflow configuration value.");
        config.AddCommand<ConfigSaveCommand>("save")
            .WithDescription("Save the current gitflow configuration to a .gitflow file.");
    });

    // Version command
    config.AddCommand<VersionCommand>("version")
        .WithDescription("Show the git-flow version information.");

    // Push command
    config.AddCommand<PushCommand>("push")
        .WithDescription("Push main, develop, and tags to remote.");

    // Feature commands
    config.AddBranch("feature", feature => {
        feature.SetDescription("Manage feature branches.");
        feature.AddCommand<FeatureListCommand>("list")
            .WithDescription("List all feature branches.");
        feature.AddCommand<FeatureStartCommand>("start")
            .WithDescription("Start a new feature branch.");
        feature.AddCommand<FeatureFinishCommand>("finish")
            .WithDescription("Finish a feature branch.");
        feature.AddCommand<FeaturePublishCommand>("publish")
            .WithDescription("Publish a feature branch to remote.");
        feature.AddCommand<FeatureTrackCommand>("track")
            .WithDescription("Track a remote feature branch.");
        feature.AddCommand<FeatureDeleteCommand>("delete")
            .WithDescription("Delete a feature branch.");
    });

    // Bugfix commands
    config.AddBranch("bugfix", bugfix => {
        bugfix.SetDescription("Manage bugfix branches.");
        bugfix.AddCommand<BugfixListCommand>("list")
            .WithDescription("List all bugfix branches.");
        bugfix.AddCommand<BugfixStartCommand>("start")
            .WithDescription("Start a new bugfix branch.");
        bugfix.AddCommand<BugfixFinishCommand>("finish")
            .WithDescription("Finish a bugfix branch.");
        bugfix.AddCommand<BugfixPublishCommand>("publish")
            .WithDescription("Publish a bugfix branch to remote.");
        bugfix.AddCommand<BugfixTrackCommand>("track")
            .WithDescription("Track a remote bugfix branch.");
        bugfix.AddCommand<BugfixDeleteCommand>("delete")
            .WithDescription("Delete a bugfix branch.");
    });

    // Release commands
    config.AddBranch("release", release => {
        release.SetDescription("Manage release branches.");
        release.AddCommand<ReleaseListCommand>("list")
            .WithDescription("List all release branches.");
        release.AddCommand<ReleaseStartCommand>("start")
            .WithDescription("Start a new release branch.");
        release.AddCommand<ReleaseFinishCommand>("finish")
            .WithDescription("Finish a release branch.");
        release.AddCommand<ReleasePublishCommand>("publish")
            .WithDescription("Publish a release branch to remote.");
        release.AddCommand<ReleaseTrackCommand>("track")
            .WithDescription("Track a remote release branch.");
        release.AddCommand<ReleaseDeleteCommand>("delete")
            .WithDescription("Delete a release branch.");
    });

    // Hotfix commands
    config.AddBranch("hotfix", hotfix => {
        hotfix.SetDescription("Manage hotfix branches.");
        hotfix.AddCommand<HotfixListCommand>("list")
            .WithDescription("List all hotfix branches.");
        hotfix.AddCommand<HotfixStartCommand>("start")
            .WithDescription("Start a new hotfix branch.");
        hotfix.AddCommand<HotfixFinishCommand>("finish")
            .WithDescription("Finish a hotfix branch.");
        hotfix.AddCommand<HotfixPublishCommand>("publish")
            .WithDescription("Publish a hotfix branch to remote.");
        hotfix.AddCommand<HotfixTrackCommand>("track")
            .WithDescription("Track a remote hotfix branch.");
        hotfix.AddCommand<HotfixDeleteCommand>("delete")
            .WithDescription("Delete a hotfix branch.");
    });

    // Support commands
    config.AddBranch("support", support => {
        support.SetDescription("Manage support branches.");
        support.AddCommand<SupportListCommand>("list")
            .WithDescription("List all support branches.");
        support.AddCommand<SupportStartCommand>("start")
            .WithDescription("Start a new support branch.");
        support.AddCommand<SupportPublishCommand>("publish")
            .WithDescription("Publish a support branch to remote.");
        support.AddCommand<SupportTrackCommand>("track")
            .WithDescription("Track a remote support branch.");
        support.AddCommand<SupportDeleteCommand>("delete")
            .WithDescription("Delete a support branch.");
    });

    config.PropagateExceptions();
});

try {
    return await app.RunAsync(args);
} catch(CommandRuntimeException ex) {
    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
    return 1;
}
