using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Track a remote release branch.
/// </summary>
public sealed class ReleaseTrackCommand : BranchTrackCommand {
    protected override IBranchService BranchService => ReleaseService;
}
