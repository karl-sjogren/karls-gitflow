using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// List all hotfix branches.
/// </summary>
public sealed class HotfixListCommand : BranchListCommand {
    protected override IBranchService BranchService => HotfixService;
}
