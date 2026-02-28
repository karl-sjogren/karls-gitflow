using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Delete a hotfix branch.
/// </summary>
public sealed class HotfixDeleteCommand : BranchDeleteCommand {
    protected override IBranchService BranchService => HotfixService;
}
