using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Track a remote hotfix branch.
/// </summary>
public sealed class HotfixTrackCommand : BranchTrackCommand {
    protected override IBranchService BranchService => HotfixService;
}
