using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Release;

/// <summary>
/// Publish a release branch to remote.
/// </summary>
public sealed class ReleasePublishCommand : BranchPublishCommand {
    protected override IBranchService BranchService => ReleaseService;
}
