namespace Karls.Gitflow.Core;

/// <summary>
/// Provides low-level git operations.
/// </summary>
public interface IGitService {
    // Repository state
    bool IsGitRepository();
    bool IsWorkingTreeClean();
    string GetRepositoryRoot();

    // Branch queries
    string GetCurrentBranchName();
    string[] GetLocalBranches();
    string[] GetRemoteBranches();
    string[] GetAllBranches();
    bool LocalBranchExists(string branchName);
    bool RemoteBranchExists(string branchName);
    bool IsBranchMerged(string branch, string targetBranch);

    // Tag queries
    bool TagExists(string tagName);
    string[] GetTags();

    // Ref queries
    bool RefExists(string refName);

    // Configuration
    GitFlowConfiguration GetGitFlowConfiguration();
    string? GetConfigValue(string key);
    void SetConfigValue(string key, string value);
    bool IsGitFlowInitialized();

    // Branch operations
    void CreateBranch(string branchName, string baseBranch);
    void CheckoutBranch(string branchName);
    void DeleteLocalBranch(string branchName, bool force = false);
    void DeleteRemoteBranch(string branchName);

    // Merge operations
    void MergeBranch(string sourceBranch, bool noFastForward = true);
    void MergeBranchSquash(string sourceBranch);

    // Tag operations
    void CreateTag(string tagName, string? message = null);
    void DeleteTag(string tagName);

    // Remote operations
    void Fetch();

    /// <summary>
    /// Pushes a branch to the remote.
    /// </summary>
    /// <param name="branchName">The branch to push.</param>
    /// <param name="setUpstream">Whether to set the upstream tracking reference.</param>
    /// <returns>Server messages (e.g., PR links, security warnings).</returns>
    string[] PushBranch(string branchName, bool setUpstream = false);

    /// <summary>
    /// Pushes all tags to the remote.
    /// </summary>
    /// <returns>Server messages (e.g., PR links, security warnings).</returns>
    string[] PushTags();
}
