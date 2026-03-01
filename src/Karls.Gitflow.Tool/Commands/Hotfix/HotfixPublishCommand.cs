using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Hotfix;

/// <summary>
/// Publish a hotfix branch to remote.
/// </summary>
public sealed class HotfixPublishCommand : BranchPublishCommand {
    protected override IBranchService BranchService => HotfixService;
}
