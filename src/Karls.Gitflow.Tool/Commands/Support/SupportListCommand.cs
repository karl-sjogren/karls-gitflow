using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// List all support branches.
/// </summary>
public sealed class SupportListCommand : BranchListCommand {
    protected override IBranchService BranchService => SupportService;
}
