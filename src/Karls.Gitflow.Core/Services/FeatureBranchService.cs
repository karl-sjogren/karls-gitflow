namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Service for managing feature branches.
/// Feature branches are created from develop and merged back into develop.
/// </summary>
public sealed class FeatureBranchService : BranchServiceBase {
    public FeatureBranchService(IGitService gitService) : base(gitService) {
    }

    /// <inheritdoc />
    public override string Prefix => Config.FeaturePrefix;

    /// <inheritdoc />
    public override string TypeName => "feature";

    /// <inheritdoc />
    protected override string DefaultBaseBranch => Config.DevelopBranch;

    /// <inheritdoc />
    public override void Finish(string name, FinishOptions? options = null) {
        FinishSimpleMerge(name, Config.DevelopBranch, options);
    }
}
