using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Start a new release branch.
/// </summary>
public sealed class ReleaseStartCommand : BranchStartCommand {
    protected override IBranchService BranchService => ReleaseService;
}
