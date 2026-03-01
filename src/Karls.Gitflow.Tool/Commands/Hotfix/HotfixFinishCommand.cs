using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Finish a hotfix branch.
/// </summary>
public sealed class HotfixFinishCommand : BranchTagFinishCommand {
    protected override IBranchService BranchService => HotfixService;
}
