using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Delete a bugfix branch.
/// </summary>
public sealed class BugfixDeleteCommand : BranchDeleteCommand {
    protected override IBranchService BranchService => BugfixService;
}
