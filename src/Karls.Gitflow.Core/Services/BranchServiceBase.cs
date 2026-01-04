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

        var progress = options.OnProgress;

        // Optionally fetch from origin
        if(options.Fetch) {
            progress?.Invoke("Fetching from origin...");
            GitService.Fetch();
        }

        // Checkout target branch and merge
        progress?.Invoke($"Merging into '{targetBranch}'...");
        GitService.CheckoutBranch(targetBranch);

        // Merge the branch
        if(options.Squash) {
            GitService.MergeBranchSquash(fullBranchName);
        } else {
            GitService.MergeBranch(fullBranchName, noFastForward: true);
        }

        progress?.Invoke($"Merged into '{targetBranch}'");

        // Delete the branch unless --keep
        if(!options.Keep) {
            progress?.Invoke($"Deleting branch '{fullBranchName}'...");
            GitService.DeleteLocalBranch(fullBranchName, force: true);

            // Delete remote if it exists
            if(GitService.RemoteBranchExists(fullBranchName)) {
                GitService.DeleteRemoteBranch(fullBranchName);
            }

            progress?.Invoke($"Deleted branch '{fullBranchName}'");
        }

        // Optionally push
        if(options.Push) {
            progress?.Invoke($"Pushing '{targetBranch}'...");
            GitService.PushBranch(targetBranch);
            progress?.Invoke($"Pushed '{targetBranch}'");
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
        var progress = options.OnProgress;

        // Optionally fetch from origin
        if(options.Fetch) {
            progress?.Invoke("Fetching from origin...");
            GitService.Fetch();
        }

        // Step 1: Merge into main
        progress?.Invoke($"Merging into '{mainBranch}'...");
        GitService.CheckoutBranch(mainBranch);
        if(options.Squash) {
            GitService.MergeBranchSquash(fullBranchName);
        } else {
            GitService.MergeBranch(fullBranchName, noFastForward: true);
        }

        progress?.Invoke($"Merged into '{mainBranch}'");

        // Step 2: Create tag on main (unless --notag)
        if(!options.NoTag) {
            if(GitService.TagExists(tagName)) {
                throw new GitFlowException($"Tag '{tagName}' already exists.");
            }

            progress?.Invoke($"Creating tag '{tagName}'...");
            var message = options.TagMessage ?? FormatTagMessage(Config.TagMessageTemplate, version);
            GitService.CreateTag(tagName, message);
            progress?.Invoke($"Created tag '{tagName}'");
        }

        // Step 3: Merge into develop (unless --nobackmerge)
        if(!options.NoBackMerge) {
            progress?.Invoke($"Merging into '{developBranch}'...");
            GitService.CheckoutBranch(developBranch);
            // Merge the tag (preferred) or main branch
            var mergeRef = options.NoTag ? mainBranch : tagName;
            GitService.MergeBranch(mergeRef, noFastForward: true);
            progress?.Invoke($"Merged into '{developBranch}'");
        }

        // Step 4: Delete the branch unless --keep
        if(!options.Keep) {
            progress?.Invoke($"Deleting branch '{fullBranchName}'...");
            GitService.DeleteLocalBranch(fullBranchName, force: true);

            if(GitService.RemoteBranchExists(fullBranchName)) {
                GitService.DeleteRemoteBranch(fullBranchName);
            }

            progress?.Invoke($"Deleted branch '{fullBranchName}'");
        }

        // Step 5: Optionally push everything
        if(options.Push) {
            progress?.Invoke($"Pushing '{mainBranch}'...");
            GitService.PushBranch(mainBranch);
            progress?.Invoke($"Pushed '{mainBranch}'");

            if(!options.NoBackMerge) {
                progress?.Invoke($"Pushing '{developBranch}'...");
                GitService.PushBranch(developBranch);
                progress?.Invoke($"Pushed '{developBranch}'");
            }

            if(!options.NoTag) {
                progress?.Invoke("Pushing tags...");
                GitService.PushTags();
                progress?.Invoke("Pushed tags");
            }
        }

        // Return to develop branch
        GitService.CheckoutBranch(developBranch);
    }

    /// <summary>
    /// Formats a tag message template with placeholders.
    /// Returns null if no template is configured (lets git handle it).
    /// </summary>
    /// <param name="template">The template with placeholders: {version}, {date}, {type}</param>
    /// <param name="version">The version being tagged</param>
    /// <returns>The formatted message, or null if no template</returns>
    protected string? FormatTagMessage(string template, string version) {
        if(string.IsNullOrEmpty(template)) {
            return null;
        }

        return template
            .Replace("{version}", version, StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", TimeProvider.System.GetLocalNow().ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{type}", TypeName, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
