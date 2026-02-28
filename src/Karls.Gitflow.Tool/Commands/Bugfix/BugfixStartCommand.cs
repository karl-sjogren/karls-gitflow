using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Start a new bugfix branch.
/// </summary>
public sealed class BugfixStartCommand : BranchStartCommand {
    protected override IBranchService BranchService => BugfixService;
}
