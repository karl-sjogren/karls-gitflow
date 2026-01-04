namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Common interface for gitflow branch operations.
/// </summary>
public interface IBranchService {
    /// <summary>
    /// Gets the branch prefix (e.g., "feature/").
    /// </summary>
    string Prefix { get; }

    /// <summary>
    /// Gets the type name for display purposes (e.g., "feature").
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// Gets the current branch name (without prefix) if on a branch of this type.
    /// Returns null if not on a branch of this type.
    /// </summary>
    string? GetCurrentBranchNameIfOnType();

    /// <summary>
    /// Resolves the branch name - uses the provided name or detects from current branch.
    /// </summary>
    /// <param name="name">The provided name, or null/empty to auto-detect.</param>
    /// <returns>The resolved branch name.</returns>
    string ResolveBranchName(string? name);

    /// <summary>
    /// Lists all branches of this type.
    /// </summary>
    /// <returns>Array of branch names without the prefix.</returns>
    string[] List();

    /// <summary>
    /// Starts a new branch.
    /// </summary>
    /// <param name="name">The branch name (without prefix).</param>
    /// <param name="baseBranch">Optional base branch to create from.</param>
    void Start(string name, string? baseBranch = null);

    /// <summary>
    /// Finishes a branch by merging and cleaning up.
    /// </summary>
    /// <param name="name">The branch name (without prefix).</param>
    /// <param name="options">Options for the finish operation.</param>
    void Finish(string name, FinishOptions? options = null);

    /// <summary>
    /// Publishes a branch to the remote.
    /// </summary>
    /// <param name="name">The branch name (without prefix).</param>
    void Publish(string name);

    /// <summary>
    /// Deletes a branch.
    /// </summary>
    /// <param name="name">The branch name (without prefix).</param>
    /// <param name="options">Options for the delete operation.</param>
    void Delete(string name, DeleteOptions? options = null);
}

/// <summary>
/// Options for finishing a branch.
/// </summary>
public sealed record FinishOptions {
    /// <summary>
    /// Whether to fetch from origin before finishing.
    /// </summary>
    public bool Fetch { get; init; }

    /// <summary>
    /// Whether to push to origin after finishing.
    /// </summary>
    public bool Push { get; init; }

    /// <summary>
    /// Whether to keep the branch after merging.
    /// </summary>
    public bool Keep { get; init; }

    /// <summary>
    /// Whether to squash commits during merge.
    /// </summary>
    public bool Squash { get; init; }

    /// <summary>
    /// Tag message for release/hotfix branches.
    /// </summary>
    public string? TagMessage { get; init; }

    /// <summary>
    /// Whether to skip tagging (for release/hotfix).
    /// </summary>
    public bool NoTag { get; init; }

    /// <summary>
    /// Whether to skip back-merge to develop (for release/hotfix).
    /// </summary>
    public bool NoBackMerge { get; init; }

    /// <summary>
    /// Optional callback for reporting progress during the finish operation.
    /// </summary>
    public Action<string>? OnProgress { get; init; }
}

/// <summary>
/// Options for deleting a branch.
/// </summary>
public sealed record DeleteOptions {
    /// <summary>
    /// Whether to force delete.
    /// </summary>
    public bool Force { get; init; }

    /// <summary>
    /// Whether to also delete the remote branch.
    /// </summary>
    public bool Remote { get; init; }
}
