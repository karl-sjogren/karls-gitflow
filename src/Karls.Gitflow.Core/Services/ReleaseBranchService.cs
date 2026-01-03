namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Service for managing release branches.
/// Release branches are created from develop and merged into both main and develop.
/// A version tag is created on main after the merge.
/// </summary>
public sealed class ReleaseBranchService : BranchServiceBase {
    public ReleaseBranchService(IGitService gitService) : base(gitService) {
    }

    /// <inheritdoc />
    public override string Prefix => Config.ReleasePrefix;

    /// <inheritdoc />
    public override string TypeName => "release";

    /// <inheritdoc />
    protected override string DefaultBaseBranch => Config.DevelopBranch;

    /// <inheritdoc />
    public override void Finish(string name, FinishOptions? options = null) {
        // For releases, the name is typically the version number
        FinishDualMerge(name, name, options);
    }
}
