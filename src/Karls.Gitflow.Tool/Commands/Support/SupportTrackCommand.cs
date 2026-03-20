using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Track a remote support branch.
/// </summary>
public sealed class SupportTrackCommand : BranchTrackCommand {
    protected override IBranchService BranchService => SupportService;
}
