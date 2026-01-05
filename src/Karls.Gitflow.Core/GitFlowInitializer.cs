namespace Karls.Gitflow.Core;

/// <summary>
/// Initializes gitflow in a repository.
/// </summary>
public sealed class GitFlowInitializer {
    private readonly IGitService _gitService;

    public GitFlowInitializer(IGitService gitService) {
        _gitService = gitService;
    }

    /// <summary>
    /// Checks if gitflow is already initialized.
    /// </summary>
    public bool IsInitialized => _gitService.IsGitFlowInitialized();

    /// <summary>
    /// Initializes gitflow with the specified configuration.
    /// </summary>
    /// <param name="config">The gitflow configuration to use.</param>
    /// <param name="force">If true, reinitialize even if already configured.</param>
    public void Initialize(GitFlowConfiguration config, bool force = false) {
        if(!_gitService.IsGitRepository()) {
            throw new GitFlowException("Not a git repository.");
        }

        if(IsInitialized && !force) {
            throw new GitFlowException("Gitflow is already initialized. Use --force to reinitialize.");
        }

        // Validate configuration
        if(!config.IsValid()) {
            throw new GitFlowException("Invalid gitflow configuration.");
        }

        // Ensure main branch exists
        EnsureMainBranchExists(config.MainBranch);

        // Ensure develop branch exists
        EnsureDevelopBranchExists(config.MainBranch, config.DevelopBranch);

        // Write configuration
        WriteConfiguration(config);
    }

    /// <summary>
    /// Initializes gitflow with default configuration.
    /// </summary>
    /// <param name="force">If true, reinitialize even if already configured.</param>
    public void InitializeWithDefaults(bool force = false) {
        Initialize(GitFlowConfiguration.Default, force);
    }

    private void EnsureMainBranchExists(string mainBranch) {
        if(_gitService.LocalBranchExists(mainBranch)) {
            return;
        }

        // Check if the branch exists on remote - if so, check it out to create local tracking branch
        if(_gitService.RemoteBranchExists(mainBranch)) {
            _gitService.CheckoutBranch(mainBranch);
            return;
        }

        // Check if we're in an empty repository
        var branches = _gitService.GetLocalBranches();
        if(branches.Length == 0) {
            throw new GitFlowException(
                $"Repository has no branches. Create an initial commit on '{mainBranch}' first.");
        }

        // Check if any common main branch names exist (locally or on remote)
        var commonMainNames = new[] { "main", "master", "production" };
        var existingMain = commonMainNames.FirstOrDefault(b =>
            _gitService.LocalBranchExists(b) || _gitService.RemoteBranchExists(b));

        if(existingMain != null && existingMain != mainBranch) {
            throw new GitFlowException(
                $"Branch '{mainBranch}' does not exist, but '{existingMain}' does. " +
                $"Consider using '{existingMain}' as your main branch.");
        }

        throw new GitFlowException($"Main branch '{mainBranch}' does not exist.");
    }

    private void EnsureDevelopBranchExists(string mainBranch, string developBranch) {
        if(_gitService.LocalBranchExists(developBranch)) {
            return;
        }

        // Create develop branch from main
        _gitService.CreateBranch(developBranch, mainBranch);
    }

    private void WriteConfiguration(GitFlowConfiguration config) {
        // Branch configuration
        _gitService.SetConfigValue("gitflow.branch.master", config.MainBranch);
        _gitService.SetConfigValue("gitflow.branch.develop", config.DevelopBranch);

        // Prefix configuration
        _gitService.SetConfigValue("gitflow.prefix.feature", config.FeaturePrefix);
        _gitService.SetConfigValue("gitflow.prefix.bugfix", config.BugfixPrefix);
        _gitService.SetConfigValue("gitflow.prefix.release", config.ReleasePrefix);
        _gitService.SetConfigValue("gitflow.prefix.hotfix", config.HotfixPrefix);
        _gitService.SetConfigValue("gitflow.prefix.support", config.SupportPrefix);
        _gitService.SetConfigValue("gitflow.prefix.versiontag", config.VersionTagPrefix);

        // Message configuration (only write if not empty)
        if(!string.IsNullOrEmpty(config.TagMessageTemplate)) {
            _gitService.SetConfigValue("gitflow.message.tag", config.TagMessageTemplate);
        }
    }
}
