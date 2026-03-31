using Spectre.Console;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Service for displaying update prompts to the user.
/// </summary>
public sealed class UpdatePromptService : IUpdatePromptService {
    private readonly IAnsiConsole _console;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdatePromptService"/>.
    /// </summary>
    /// <param name="console">The console to use for output. When <c>null</c>, <see cref="AnsiConsole.Console"/> is used.</param>
    public UpdatePromptService(IAnsiConsole? console = null) {
        _console = console ?? AnsiConsole.Console;
    }

    /// <summary>
    /// Prompts the user to update the tool.
    /// </summary>
    /// <param name="currentVersion">The current version of the tool.</param>
    /// <param name="latestVersion">The latest available version.</param>
    /// <returns>The user's choice.</returns>
    public UpdatePromptResult PromptUser(Version currentVersion, Version latestVersion) {
        _console.MarkupLine($"[yellow]A new version of karls-gitflow is available![/]");
        _console.MarkupLine($"Current version: [red]{currentVersion}[/]");
        _console.MarkupLine($"Latest version:  [green]{latestVersion}[/]");
        _console.WriteLine();

        var choice = _console.Prompt(
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
        _console.WriteLine();
        if(installType == InstallType.Msi) {
            _console.MarkupLine("[green]To update karls-gitflow, download the latest installer from:[/]");
            _console.MarkupLine("[cyan]  https://github.com/karl-sjogren/karls-gitflow/releases/latest[/]");
        } else if(installType == InstallType.Homebrew) {
            _console.MarkupLine("[green]To update karls-gitflow, run:[/]");
            _console.MarkupLine("[cyan]  brew upgrade karl-sjogren/tap/karls-gitflow[/]");
        } else {
            _console.MarkupLine("[green]To update karls-gitflow, run:[/]");
            _console.MarkupLine("[cyan]  dotnet tool update -g Karls.Gitflow.Tool[/]");
        }

        _console.WriteLine();
    }
}
