namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Result of checking for an available update.
/// </summary>
public enum UpdateCheckResult {
    /// <summary>
    /// The check was skipped by automatic-check settings or timing.
    /// </summary>
    Skipped,

    /// <summary>
    /// The latest version could not be retrieved or the check failed.
    /// </summary>
    Failed,

    /// <summary>
    /// No newer version is available.
    /// </summary>
    UpToDate,

    /// <summary>
    /// A newer version is available.
    /// </summary>
    UpdateAvailable,

    /// <summary>
    /// A newer version is available and update instructions were displayed.
    /// </summary>
    UpdateNow
}
