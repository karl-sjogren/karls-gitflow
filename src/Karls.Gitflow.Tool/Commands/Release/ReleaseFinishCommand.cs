using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Finish a release branch.
/// </summary>
public sealed class ReleaseFinishCommand : BranchTagFinishCommand {
    protected override IBranchService BranchService => ReleaseService;
}
