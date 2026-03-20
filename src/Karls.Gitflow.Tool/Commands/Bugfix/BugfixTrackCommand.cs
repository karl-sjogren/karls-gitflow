using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Track a remote bugfix branch.
/// </summary>
public sealed class BugfixTrackCommand : BranchTrackCommand {
    protected override IBranchService BranchService => BugfixService;
}
