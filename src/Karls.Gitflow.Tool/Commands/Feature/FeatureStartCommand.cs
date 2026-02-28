using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Start a new feature branch.
/// </summary>
public sealed class FeatureStartCommand : BranchStartCommand {
    protected override IBranchService BranchService => FeatureService;
}
