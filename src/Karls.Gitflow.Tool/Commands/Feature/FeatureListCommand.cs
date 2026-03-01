using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// List all feature branches.
/// </summary>
public sealed class FeatureListCommand : BranchListCommand {
    protected override IBranchService BranchService => FeatureService;
}
