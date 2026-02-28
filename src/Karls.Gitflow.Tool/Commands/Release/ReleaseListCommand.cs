using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// List all release branches.
/// </summary>
public sealed class ReleaseListCommand : BranchListCommand {
    protected override IBranchService BranchService => ReleaseService;
}
