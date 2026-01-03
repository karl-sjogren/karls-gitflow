namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Base class for gitflow branch services with shared logic.
/// </summary>
public abstract class BranchServiceBase : IBranchService {
    protected readonly IGitService GitService;
    protected GitFlowConfiguration Config => GitService.GetGitFlowConfiguration();

    protected BranchServiceBase(IGitService gitService) {
        GitService = gitService;
    }

    /// <inheritdoc />
    public abstract string Prefix { get; }

    /// <inheritdoc />
    public abstract string TypeName { get; }

    /// <summary>
    /// Gets the default base branch for new branches.
    /// </summary>
    protected abstract string DefaultBaseBranch { get; }

    /// <summary>
    /// Gets the full branch name with prefix.
    /// </summary>
    protected string GetFullBranchName(string name) => $"{Prefix}{name}";

    /// <summary>
    /// Gets the current branch name (without prefix) if on a branch of this type.
    /// Returns null if not on a branch of this type.
    /// </summary>
    public string? GetCurrentBranchNameIfOnType() {
        var currentBranch = GitService.GetCurrentBranchName();
        if(currentBranch.StartsWith(Prefix, StringComparison.Ordinal)) {
            return currentBranch[Prefix.Length..];
        }

        return null;
    }

    /// <summary>
    /// Resolves the branch name - uses the provided name or detects from current branch.
    /// </summary>
    /// <param name="name">The provided name, or null/empty to auto-detect.</param>
    /// <returns>The resolved branch name.</returns>
    /// <exception cref="GitFlowException">Thrown if no name provided and not on a branch of this type.</exception>
    public string ResolveBranchName(string? name) {
        if(!string.IsNullOrWhiteSpace(name)) {
            return name;
        }

        var currentName = GetCurrentBranchNameIfOnType();
        if(currentName == null) {
            throw new GitFlowException(
                $"Not on a {TypeName} branch. Please specify a branch name or switch to a {TypeName} branch.");
        }

        return currentName;
    }

    #region Validation

    protected void ValidateGitRepository() {
        if(!GitService.IsGitRepository()) {
            throw new GitFlowException("Not a git repository.");
        }
    }

    protected void ValidateGitFlowInitialized() {
        if(!GitService.IsGitFlowInitialized()) {
            throw new GitFlowException("Gitflow is not initialized. Run 'git-flow init' first.");
        }
    }

    protected void ValidateWorkingTreeClean() {
        if(!GitService.IsWorkingTreeClean()) {
            throw new GitFlowException("Working tree contains uncommitted changes. Please commit or stash them first.");
        }
    }

    protected void ValidateBranchDoesNotExist(string branchName) {
        if(GitService.LocalBranchExists(branchName)) {
            throw new GitFlowException($"Branch '{branchName}' already exists locally.");
        }

        if(GitService.RemoteBranchExists(branchName)) {
            throw new GitFlowException($"Branch '{branchName}' already exists on remote.");
        }
    }

    protected void ValidateBranchExists(string branchName) {
        if(!GitService.LocalBranchExists(branchName)) {
            throw new GitFlowException($"Branch '{branchName}' does not exist.");
        }
    }

    protected void ValidateBaseBranchExists(string baseBranch) {
        if(!GitService.LocalBranchExists(baseBranch) && !GitService.RemoteBranchExists(baseBranch)) {
            throw new GitFlowException($"Base branch '{baseBranch}' does not exist.");
        }
    }

    protected void ValidateAll() {
        ValidateGitRepository();
        ValidateGitFlowInitialized();
    }

    #endregion

    #region IBranchService Implementation

    /// <inheritdoc />
    public virtual string[] List() {
        ValidateAll();

        var branches = GitService.GetLocalBranches();
        return branches
            .Where(b => b.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(b => b[Prefix.Length..])
            .ToArray();
    }

    /// <inheritdoc />
    public virtual void Start(string name, string? baseBranch = null) {
        ValidateAll();
        ValidateWorkingTreeClean();

        var fullBranchName = GetFullBranchName(name);
        var baseRef = baseBranch ?? DefaultBaseBranch;

        ValidateBranchDoesNotExist(fullBranchName);
        ValidateBaseBranchExists(baseRef);

        GitService.CreateBranch(fullBranchName, baseRef);
    }

    /// <inheritdoc />
    public abstract void Finish(string name, FinishOptions? options = null);

    /// <inheritdoc />
    public virtual void Publish(string name) {
        ValidateAll();

        var fullBranchName = GetFullBranchName(name);
        ValidateBranchExists(fullBranchName);

        if(GitService.RemoteBranchExists(fullBranchName)) {
            throw new GitFlowException($"Branch '{fullBranchName}' already exists on remote.");
        }

        GitService.PushBranch(fullBranchName, setUpstream: true);
    }

    /// <inheritdoc />
    public virtual void Delete(string name, DeleteOptions? options = null) {
        ValidateAll();

        var fullBranchName = GetFullBranchName(name);
        options ??= new DeleteOptions();

        // Check if the branch exists
        var localExists = GitService.LocalBranchExists(fullBranchName);
        var remoteExists = GitService.RemoteBranchExists(fullBranchName);

        if(!localExists && !remoteExists) {
            throw new GitFlowException($"Branch '{fullBranchName}' does not exist.");
        }

        // Check if we're on the branch we're trying to delete
        var currentBranch = GitService.GetCurrentBranchName();
        if(currentBranch == fullBranchName) {
            GitService.CheckoutBranch(DefaultBaseBranch);
        }

        // Delete local branch
        if(localExists) {
            GitService.DeleteLocalBranch(fullBranchName, options.Force);
        }

        // Delete remote branch
        if(options.Remote && remoteExists) {
            GitService.DeleteRemoteBranch(fullBranchName);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Performs a simple merge finish workflow (feature/bugfix pattern).
    /// </summary>
    protected void FinishSimpleMerge(string name, string targetBranch, FinishOptions? options = null) {
        ValidateAll();
        ValidateWorkingTreeClean();

        var fullBranchName = GetFullBranchName(name);
        options ??= new FinishOptions();

        ValidateBranchExists(fullBranchName);

        // Optionally fetch from origin
        if(options.Fetch) {
            GitService.Fetch();
        }

        // Checkout target branch
        GitService.CheckoutBranch(targetBranch);

        // Merge the branch
        if(options.Squash) {
            GitService.MergeBranchSquash(fullBranchName);
        } else {
            GitService.MergeBranch(fullBranchName, noFastForward: true);
        }

        // Delete the branch unless --keep
        if(!options.Keep) {
            GitService.DeleteLocalBranch(fullBranchName, force: true);

            // Delete remote if it exists
            if(GitService.RemoteBranchExists(fullBranchName)) {
                GitService.DeleteRemoteBranch(fullBranchName);
            }
        }

        // Optionally push
        if(options.Push) {
            GitService.PushBranch(targetBranch);
        }
    }

    /// <summary>
    /// Performs a dual merge finish workflow (release/hotfix pattern).
    /// Merges to main, creates tag, then merges to develop.
    /// </summary>
    protected void FinishDualMerge(string name, string version, FinishOptions? options = null) {
        ValidateAll();
        ValidateWorkingTreeClean();

        var fullBranchName = GetFullBranchName(name);
        options ??= new FinishOptions();

        ValidateBranchExists(fullBranchName);

        var mainBranch = Config.MainBranch;
        var developBranch = Config.DevelopBranch;
        var tagName = $"{Config.VersionTagPrefix}{version}";

        // Optionally fetch from origin
        if(options.Fetch) {
            GitService.Fetch();
        }

        // Step 1: Merge into main
        GitService.CheckoutBranch(mainBranch);
        if(options.Squash) {
            GitService.MergeBranchSquash(fullBranchName);
        } else {
            GitService.MergeBranch(fullBranchName, noFastForward: true);
        }

        // Step 2: Create tag on main (unless --notag)
        if(!options.NoTag) {
            if(GitService.TagExists(tagName)) {
                throw new GitFlowException($"Tag '{tagName}' already exists.");
            }

            var message = options.TagMessage ?? $"Release {version}";
            GitService.CreateTag(tagName, message);
        }

        // Step 3: Merge into develop (unless --nobackmerge)
        if(!options.NoBackMerge) {
            GitService.CheckoutBranch(developBranch);
            // Merge the tag (preferred) or main branch
            var mergeRef = options.NoTag ? mainBranch : tagName;
            GitService.MergeBranch(mergeRef, noFastForward: true);
        }

        // Step 4: Delete the branch unless --keep
        if(!options.Keep) {
            GitService.DeleteLocalBranch(fullBranchName, force: true);

            if(GitService.RemoteBranchExists(fullBranchName)) {
                GitService.DeleteRemoteBranch(fullBranchName);
            }
        }

        // Step 5: Optionally push everything
        if(options.Push) {
            GitService.PushBranch(mainBranch);
            if(!options.NoBackMerge) {
                GitService.PushBranch(developBranch);
            }

            if(!options.NoTag) {
                GitService.PushTags();
            }
        }

        // Return to develop branch
        GitService.CheckoutBranch(developBranch);
    }

    #endregion
}
