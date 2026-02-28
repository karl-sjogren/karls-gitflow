using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Delete a support branch.
/// </summary>
public sealed class SupportDeleteCommand : BranchDeleteCommand {
    protected override IBranchService BranchService => SupportService;
}
