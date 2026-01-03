namespace Karls.Gitflow.Core;

/// <summary>
/// Represents the gitflow configuration for a repository.
/// </summary>
public sealed record GitFlowConfiguration {
    /// <summary>
    /// The name of the main/production branch (e.g., "main" or "master").
    /// </summary>
    public required string MainBranch { get; init; }

    /// <summary>
    /// The name of the develop/integration branch.
    /// </summary>
    public required string DevelopBranch { get; init; }

    /// <summary>
    /// The prefix for feature branches (e.g., "feature/").
    /// </summary>
    public required string FeaturePrefix { get; init; }

    /// <summary>
    /// The prefix for bugfix branches (e.g., "bugfix/").
    /// </summary>
    public required string BugfixPrefix { get; init; }

    /// <summary>
    /// The prefix for release branches (e.g., "release/").
    /// </summary>
    public required string ReleasePrefix { get; init; }

    /// <summary>
    /// The prefix for hotfix branches (e.g., "hotfix/").
    /// </summary>
    public required string HotfixPrefix { get; init; }

    /// <summary>
    /// The prefix for support branches (e.g., "support/").
    /// </summary>
    public required string SupportPrefix { get; init; }

    /// <summary>
    /// The prefix for version tags (e.g., "v" for tags like "v1.0.0").
    /// </summary>
    public string VersionTagPrefix { get; init; } = string.Empty;

    /// <summary>
    /// The template for tag messages. Supports placeholders: {version}, {date}, {type}.
    /// If empty, git creates a lightweight tag (no message).
    /// </summary>
    public string TagMessageTemplate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default gitflow configuration.
    /// </summary>
    public static GitFlowConfiguration Default => new() {
        MainBranch = DefaultValues.MainBranch,
        DevelopBranch = DefaultValues.DevelopBranch,
        FeaturePrefix = DefaultValues.FeaturePrefix,
        BugfixPrefix = DefaultValues.BugfixPrefix,
        ReleasePrefix = DefaultValues.ReleasePrefix,
        HotfixPrefix = DefaultValues.HotfixPrefix,
        SupportPrefix = DefaultValues.SupportPrefix,
        VersionTagPrefix = DefaultValues.VersionTagPrefix,
        TagMessageTemplate = DefaultValues.TagMessageTemplate
    };

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>True if all required fields are set.</returns>
    public bool IsValid() {
        return !string.IsNullOrWhiteSpace(MainBranch)
            && !string.IsNullOrWhiteSpace(DevelopBranch)
            && !string.IsNullOrWhiteSpace(FeaturePrefix)
            && !string.IsNullOrWhiteSpace(BugfixPrefix)
            && !string.IsNullOrWhiteSpace(ReleasePrefix)
            && !string.IsNullOrWhiteSpace(HotfixPrefix)
            && !string.IsNullOrWhiteSpace(SupportPrefix);
    }

    /// <summary>
    /// Default values for gitflow configuration.
    /// </summary>
    public static class DefaultValues {
        public const string MainBranch = "main";
        public const string DevelopBranch = "develop";
        public const string FeaturePrefix = "feature/";
        public const string BugfixPrefix = "bugfix/";
        public const string ReleasePrefix = "release/";
        public const string HotfixPrefix = "hotfix/";
        public const string SupportPrefix = "support/";
        public const string VersionTagPrefix = "";
        public const string TagMessageTemplate = "";
    }
}
