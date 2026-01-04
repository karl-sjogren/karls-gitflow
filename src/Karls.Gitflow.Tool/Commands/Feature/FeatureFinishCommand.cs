using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Feature;

/// <summary>
/// Finish a feature branch.
/// </summary>
public sealed class FeatureFinishCommand : GitFlowCommand<FinishSettings> {
    public override int Execute(CommandContext context, FinishSettings settings, CancellationToken cancellationToken) {
        return ExecuteSafe(() => {
            var name = FeatureService.ResolveBranchName(settings.Name);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash,
                OnProgress = CreateProgressCallback(settings.Quiet)
            };

            FeatureService.Finish(name, options);

            if(!settings.Quiet) {
                WriteSuccess($"Finished feature branch '{FeatureService.Prefix}{name}'");
            }
        });
    }
}
