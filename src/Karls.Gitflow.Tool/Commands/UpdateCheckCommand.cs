using System.Reflection;
using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Checks for tool updates immediately.
/// </summary>
public sealed class UpdateCheckCommand : AsyncCommand<UpdateCheckCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        var console = GitFlowContext.EffectiveConsole;
        console.MarkupLine("[blue]Checking for updates...[/]");

        using var nugetClient = new NuGetApiClient();
        var gitService = new GitService(new GitExecutor());
        var promptService = new UpdatePromptService(console);
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        var updateChecker = new UpdateChecker(gitService, nugetClient, promptService, currentVersion);

        var result = await updateChecker.ForceCheckForUpdatesAsync(cancellationToken);

        switch(result) {
            case UpdateCheckResult.Failed:
                console.MarkupLine("[red]Failed to check for updates.[/]");
                return 1;

            case UpdateCheckResult.UpToDate:
                console.MarkupLine($"[green]You are using the latest version ({currentVersion}).[/]");
                return 0;

            case UpdateCheckResult.UpdateAvailable:
            case UpdateCheckResult.UpdateNow:
                return 0;

            case UpdateCheckResult.Skipped:
                console.MarkupLine("[yellow]Update check was skipped.[/]");
                return 0;

            default:
                return 1;
        }
    }
}
