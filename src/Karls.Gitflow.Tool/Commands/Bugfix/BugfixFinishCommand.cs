using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Bugfix;

/// <summary>
/// Finish a bugfix branch.
/// </summary>
public sealed class BugfixFinishCommand : BranchSimpleFinishCommand {
    protected override IBranchService BranchService => BugfixService;
}
