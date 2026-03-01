using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Publish a feature branch to remote.
/// </summary>
public sealed class FeaturePublishCommand : BranchPublishCommand {
    protected override IBranchService BranchService => FeatureService;
}
