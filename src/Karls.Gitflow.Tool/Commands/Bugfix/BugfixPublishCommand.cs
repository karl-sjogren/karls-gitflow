using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Publish a bugfix branch to remote.
/// </summary>
public sealed class BugfixPublishCommand : BranchPublishCommand {
    protected override IBranchService BranchService => BugfixService;
}
