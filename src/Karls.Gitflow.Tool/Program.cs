using Karls.Gitflow.Tool.Commands;
using Karls.Gitflow.Tool.Commands.Bugfix;
using Karls.Gitflow.Tool.Commands.Feature;
using Karls.Gitflow.Tool.Commands.Hotfix;
using Karls.Gitflow.Tool.Commands.Release;
using Karls.Gitflow.Tool.Commands.Support;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config => {
    config.SetApplicationName("git-flow");
    config.SetApplicationVersion("1.0.0");

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
    });

    // Version command
    config.AddCommand<VersionCommand>("version")
        .WithDescription("Show the git-flow version information.");

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
        support.AddCommand<SupportDeleteCommand>("delete")
            .WithDescription("Delete a support branch.");
    });

    config.PropagateExceptions();
});

return app.Run(args);
