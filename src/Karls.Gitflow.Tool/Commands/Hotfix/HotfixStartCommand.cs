using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Start a new hotfix branch.
/// </summary>
public sealed class HotfixStartCommand : BranchStartCommand {
    protected override IBranchService BranchService => HotfixService;
}
