using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// List all bugfix branches.
/// </summary>
public sealed class BugfixListCommand : BranchListCommand {
    protected override IBranchService BranchService => BugfixService;
}
