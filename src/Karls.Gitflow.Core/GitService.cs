namespace Karls.Gitflow.Core;

/// <summary>
/// Provides git operations by executing git commands.
/// </summary>
public sealed class GitService : IGitService {
    private readonly IGitExecutor _gitExecutor;

    public GitService(IGitExecutor gitExecutor) {
        _gitExecutor = gitExecutor;
    }

    #region Repository State

    public bool IsGitRepository() {
        var result = _gitExecutor.Execute("rev-parse --is-inside-work-tree");
        return result.ExitCode == 0 && result.Output.Length == 1 && result.Output[0] == "true";
    }

    public bool IsWorkingTreeClean() {
        var result = _gitExecutor.Execute("status --porcelain");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get working tree status.");
        }

        return result.Output.Length == 0;
    }

    public string GetRepositoryRoot() {
        var result = _gitExecutor.Execute("rev-parse --show-toplevel");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get repository root.");
        }

        return result.Output[0];
    }

    #endregion

    #region Branch Queries

    public string GetCurrentBranchName() {
        var result = _gitExecutor.Execute("rev-parse --abbrev-ref HEAD");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get current branch name.");
        }

        return result.Output[0];
    }

    public string[] GetLocalBranches() {
        var result = _gitExecutor.Execute("for-each-ref --sort=refname --format=%(refname:short) refs/heads");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get local branches.");
        }

        return result.Output;
    }

    public string[] GetRemoteBranches() {
        var result = _gitExecutor.Execute("for-each-ref --sort=refname --format=%(refname:short) refs/remotes/origin");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get remote branches.");
        }

        // Strip "origin/" prefix from remote branches
        return result.Output
            .Select(b => b.StartsWith("origin/", StringComparison.Ordinal) ? b[7..] : b)
            .Where(b => b != "HEAD")
            .ToArray();
    }

    public string[] GetAllBranches() {
        var local = GetLocalBranches();
        var remote = GetRemoteBranches();
        return local.Union(remote).OrderBy(b => b).ToArray();
    }

    public bool LocalBranchExists(string branchName) {
        var result = _gitExecutor.Execute($"show-ref --verify --quiet refs/heads/{branchName}");
        return result.ExitCode == 0;
    }

    public bool RemoteBranchExists(string branchName) {
        var result = _gitExecutor.Execute($"show-ref --verify --quiet refs/remotes/origin/{branchName}");
        return result.ExitCode == 0;
    }

    public bool IsBranchMerged(string branch, string targetBranch) {
        var result = _gitExecutor.Execute($"branch --merged {targetBranch}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to check if branch '{branch}' is merged into '{targetBranch}'.");
        }

        return result.Output.Any(b => b.Trim().TrimStart('*').Trim() == branch);
    }

    #endregion

    #region Tag Queries

    public bool TagExists(string tagName) {
        var result = _gitExecutor.Execute($"show-ref --verify --quiet refs/tags/{tagName}");
        return result.ExitCode == 0;
    }

    public string[] GetTags() {
        var result = _gitExecutor.Execute("tag --list --sort=-version:refname");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to get tags.");
        }

        return result.Output;
    }

    #endregion

    #region Ref Queries

    public bool RefExists(string refName) {
        // Check if it's a valid commit-ish (branch, tag, or commit hash)
        var result = _gitExecutor.Execute($"rev-parse --verify --quiet {refName}");
        return result.ExitCode == 0;
    }

    #endregion

    #region Configuration

    public string? GetConfigValue(string key) {
        var result = _gitExecutor.Execute($"config --get {key}");
        if(result.ExitCode != 0) {
            return null;
        }

        return result.Output.Length > 0 ? result.Output[0] : null;
    }

    public void SetConfigValue(string key, string value) {
        var result = _gitExecutor.Execute($"config {key} \"{value}\"");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to set config value '{key}'.");
        }
    }

    public bool IsGitFlowInitialized() {
        // Check if the essential gitflow config keys exist
        var mainBranch = GetConfigValue("gitflow.branch.master");
        var developBranch = GetConfigValue("gitflow.branch.develop");
        return mainBranch != null && developBranch != null;
    }

    public GitFlowConfiguration GetGitFlowConfiguration() {
        return new GitFlowConfiguration {
            MainBranch = GetConfigValue("gitflow.branch.master") ?? GitFlowConfiguration.DefaultValues.MainBranch,
            DevelopBranch = GetConfigValue("gitflow.branch.develop") ?? GitFlowConfiguration.DefaultValues.DevelopBranch,
            FeaturePrefix = GetConfigValue("gitflow.prefix.feature") ?? GitFlowConfiguration.DefaultValues.FeaturePrefix,
            BugfixPrefix = GetConfigValue("gitflow.prefix.bugfix") ?? GitFlowConfiguration.DefaultValues.BugfixPrefix,
            ReleasePrefix = GetConfigValue("gitflow.prefix.release") ?? GitFlowConfiguration.DefaultValues.ReleasePrefix,
            HotfixPrefix = GetConfigValue("gitflow.prefix.hotfix") ?? GitFlowConfiguration.DefaultValues.HotfixPrefix,
            SupportPrefix = GetConfigValue("gitflow.prefix.support") ?? GitFlowConfiguration.DefaultValues.SupportPrefix,
            VersionTagPrefix = GetConfigValue("gitflow.prefix.versiontag") ?? GitFlowConfiguration.DefaultValues.VersionTagPrefix,
            TagMessageTemplate = GetConfigValue("gitflow.message.tag") ?? GitFlowConfiguration.DefaultValues.TagMessageTemplate
        };
    }

    #endregion

    #region Branch Operations

    public void CreateBranch(string branchName, string baseBranch) {
        var result = _gitExecutor.Execute($"checkout -b {branchName} {baseBranch}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to create branch '{branchName}' from '{baseBranch}'.");
        }
    }

    public void CheckoutBranch(string branchName) {
        var result = _gitExecutor.Execute($"checkout {branchName}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to checkout branch '{branchName}'.");
        }
    }

    public void DeleteLocalBranch(string branchName, bool force = false) {
        var flag = force ? "-D" : "-d";
        var result = _gitExecutor.Execute($"branch {flag} {branchName}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to delete local branch '{branchName}'.");
        }
    }

    public void DeleteRemoteBranch(string branchName) {
        var result = _gitExecutor.Execute($"push origin --delete {branchName}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to delete remote branch '{branchName}'.");
        }
    }

    #endregion

    #region Merge Operations

    public void MergeBranch(string sourceBranch, bool noFastForward = true) {
        var command = noFastForward ? $"merge --no-ff {sourceBranch}" : $"merge {sourceBranch}";
        var result = _gitExecutor.Execute(command);
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to merge branch '{sourceBranch}'.");
        }
    }

    public void MergeBranchSquash(string sourceBranch) {
        var result = _gitExecutor.Execute($"merge --squash {sourceBranch}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to squash merge branch '{sourceBranch}'.");
        }

        // Squash merge requires a commit
        var commitResult = _gitExecutor.Execute($"commit -m \"Squashed commit from {sourceBranch}\"");
        if(commitResult.ExitCode != 0) {
            throw new GitException($"Failed to commit squash merge from '{sourceBranch}'.");
        }
    }

    #endregion

    #region Tag Operations

    public void CreateTag(string tagName, string? message = null) {
        string command;
        if(string.IsNullOrEmpty(message)) {
            command = $"tag {tagName}";
        } else {
            command = $"tag -a {tagName} -m \"{message}\"";
        }

        var result = _gitExecutor.Execute(command);
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to create tag '{tagName}'.");
        }
    }

    public void DeleteTag(string tagName) {
        var result = _gitExecutor.Execute($"tag -d {tagName}");
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to delete tag '{tagName}'.");
        }
    }

    #endregion

    #region Remote Operations

    public void Fetch() {
        var result = _gitExecutor.Execute("fetch origin");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to fetch from origin.");
        }
    }

    public void PushBranch(string branchName, bool setUpstream = false) {
        var command = setUpstream ? $"push -u origin {branchName}" : $"push origin {branchName}";
        var result = _gitExecutor.Execute(command);
        if(result.ExitCode != 0) {
            throw new GitException($"Failed to push branch '{branchName}' to origin.");
        }
    }

    public void PushTags() {
        var result = _gitExecutor.Execute("push origin --tags");
        if(result.ExitCode != 0) {
            throw new GitException("Failed to push tags to origin.");
        }
    }

    #endregion
}

/// <summary>
/// Abstraction for executing git commands.
/// </summary>
public interface IGitExecutor {
    GitExecutorResult Execute(string command);
}

/// <summary>
/// Result of executing a git command.
/// </summary>
/// <param name="Output">The output lines from the command.</param>
/// <param name="ExitCode">The exit code of the command.</param>
public sealed record GitExecutorResult(string[] Output, int ExitCode);
