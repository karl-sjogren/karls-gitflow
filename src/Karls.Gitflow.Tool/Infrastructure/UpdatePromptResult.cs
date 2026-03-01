namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Represents the user's response to an update prompt.
/// </summary>
public enum UpdatePromptResult {
    /// <summary>
    /// User wants to update now (exit and show instructions).
    /// </summary>
    UpdateNow,

    /// <summary>
    /// User wants to be reminded later (in 14 days).
    /// </summary>
    RemindLater,

    /// <summary>
    /// User doesn't want to be asked again (disable update checks).
    /// </summary>
    DontAskAgain
}
