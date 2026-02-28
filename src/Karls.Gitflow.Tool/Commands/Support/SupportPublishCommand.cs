using Karls.Gitflow.Core.Services;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Publish a support branch to remote.
/// </summary>
public sealed class SupportPublishCommand : BranchPublishCommand {
    protected override IBranchService BranchService => SupportService;
}
