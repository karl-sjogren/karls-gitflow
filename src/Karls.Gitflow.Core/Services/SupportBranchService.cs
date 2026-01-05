namespace Karls.Gitflow.Core.Services;

/// <summary>
/// Service for managing support branches.
/// Support branches are long-lived branches for maintaining older versions.
/// They do not have a finish operation.
/// </summary>
public sealed class SupportBranchService : BranchServiceBase {
    public SupportBranchService(IGitService gitService) : base(gitService) {
    }

    /// <inheritdoc />
    public override string Prefix => Config.SupportPrefix;

    /// <inheritdoc />
    public override string TypeName => "support";

    /// <inheritdoc />
    protected override string DefaultBaseBranch => Config.MainBranch;

    /// <inheritdoc />
    /// <remarks>
    /// Support branches require an explicit base branch (typically a tag or specific commit).
    /// </remarks>
    public override void Start(string name, string? baseBranch = null) {
        if(string.IsNullOrWhiteSpace(baseBranch)) {
            throw new GitFlowException("Support branches require a base branch (typically a tag or commit).");
        }

        base.Start(name, baseBranch);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Support branches are long-lived and do not have a finish operation.
    /// </remarks>
    public override void Finish(string name, FinishOptions? options = null) {
        throw new GitFlowException("Support branches do not have a finish operation. They are long-lived branches.");
    }

    /// <inheritdoc />
    /// <remarks>
    /// Support branches are typically not published in the traditional sense.
    /// </remarks>
    public override string[] Publish(string name) {
        ValidateAll();

        var fullBranchName = GetFullBranchName(name);
        ValidateBranchExists(fullBranchName);

        return GitService.PushBranch(fullBranchName, setUpstream: true);
    }
}
