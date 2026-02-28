using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Delete a release branch.
/// </summary>
public sealed class ReleaseDeleteCommand : BranchDeleteCommand {
    protected override IBranchService BranchService => ReleaseService;
}
