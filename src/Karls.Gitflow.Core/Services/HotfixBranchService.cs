namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Service for managing hotfix branches.
/// Hotfix branches are created from main and merged into both main and develop.
/// A version tag is created on main after the merge.
/// </summary>
public sealed class HotfixBranchService : BranchServiceBase {
    public HotfixBranchService(IGitService gitService) : base(gitService) {
    }

    /// <inheritdoc />
    public override string Prefix => Config.HotfixPrefix;

    /// <inheritdoc />
    public override string TypeName => "hotfix";

    /// <inheritdoc />
    protected override string DefaultBaseBranch => Config.MainBranch;

    /// <inheritdoc />
    public override void Finish(string name, FinishOptions? options = null) {
        // For hotfixes, the name is typically the version number
        FinishDualMerge(name, name, options);
    }
}
