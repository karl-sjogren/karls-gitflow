using Spectre.Console;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Service for displaying update prompts to the user.
/// </summary>
public sealed class UpdatePromptService : IUpdatePromptService {
    /// <summary>
    /// Prompts the user to update the tool.
    /// </summary>
    /// <param name="currentVersion">The current version of the tool.</param>
    /// <param name="latestVersion">The latest available version.</param>
    /// <returns>The user's choice.</returns>
    public UpdatePromptResult PromptUser(Version currentVersion, Version latestVersion) {
        AnsiConsole.MarkupLine($"[yellow]A new version of karls-gitflow is available![/]");
        AnsiConsole.MarkupLine($"Current version: [red]{currentVersion}[/]");
        AnsiConsole.MarkupLine($"Latest version:  [green]{latestVersion}[/]");
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Would you like to update?")
                .AddChoices(new[] {
                    "Yes, update now",
                    "No, remind me later",
                    "Don't ask again"
                })
        );

        return choice switch {
            "Yes, update now" => UpdatePromptResult.UpdateNow,
            "No, remind me later" => UpdatePromptResult.RemindLater,
            "Don't ask again" => UpdatePromptResult.DontAskAgain,
            _ => UpdatePromptResult.RemindLater
        };
    }

    /// <summary>
    /// Displays instructions for updating the tool.
    /// </summary>
    /// <param name="installType">The type of installation, used to show appropriate update instructions.</param>
    public void DisplayUpdateInstructions(InstallType installType) {
        AnsiConsole.WriteLine();
        if(installType == InstallType.Msi) {
            AnsiConsole.MarkupLine("[green]To update karls-gitflow, download the latest installer from:[/]");
            AnsiConsole.MarkupLine("[cyan]  https://github.com/karl-sjogren/karls-gitflow/releases/latest[/]");
        } else {
            AnsiConsole.MarkupLine("[green]To update karls-gitflow, run:[/]");
            AnsiConsole.MarkupLine("[cyan]  dotnet tool update -g Karls.Gitflow.Tool[/]");
        }

        AnsiConsole.WriteLine();
    }
}
