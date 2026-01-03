using Karls.Gitflow.Core.Services;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Cli.Commands.Feature;

/// <summary>
/// Finish a feature branch.
/// </summary>
public sealed class FeatureFinishCommand : GitFlowCommand<FinishSettings> {
    public override int Execute(CommandContext context, FinishSettings settings) {
        return ExecuteSafe(() => {
            var name = FeatureService.ResolveBranchName(settings.Name);
            var options = new FinishOptions {
                Fetch = settings.Fetch,
                Push = settings.Push,
                Keep = settings.Keep,
                Squash = settings.Squash
            };

            FeatureService.Finish(name, options);
            WriteSuccess($"Finished feature branch '{FeatureService.Prefix}{name}'");
        });
    }
}
