namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Interface for displaying update prompts to the user.
/// </summary>
public interface IUpdatePromptService {
    /// <summary>
    /// Prompts the user to update the tool.
    /// </summary>
    /// <param name="currentVersion">The current version of the tool.</param>
    /// <param name="latestVersion">The latest available version.</param>
    /// <returns>The user's choice.</returns>
    UpdatePromptResult PromptUser(Version currentVersion, Version latestVersion);

    /// <summary>
    /// Displays instructions for updating the tool.
    /// </summary>
    void DisplayUpdateInstructions();
}
