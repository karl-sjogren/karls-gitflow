using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Finish a feature branch.
/// </summary>
public sealed class FeatureFinishCommand : BranchSimpleFinishCommand {
    protected override IBranchService BranchService => FeatureService;
}
