using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Delete a feature branch.
/// </summary>
public sealed class FeatureDeleteCommand : BranchDeleteCommand {
    protected override IBranchService BranchService => FeatureService;
}
