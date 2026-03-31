using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Track a remote feature branch.
/// </summary>
public sealed class FeatureTrackCommand : BranchTrackCommand {
    protected override IBranchService BranchService => FeatureService;
}
