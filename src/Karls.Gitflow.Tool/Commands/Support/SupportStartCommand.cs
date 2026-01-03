using System.ComponentModel;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands.Support;

/// <summary>
/// Start a new support branch.
/// </summary>
public sealed class SupportStartCommand : GitFlowCommand<SupportStartCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandArgument(0, "<name>")]
        [Description("Name for the support branch (typically a version number)")]
        public string Name { get; set; } = string.Empty;

        [CommandArgument(1, "<base>")]
        [Description("Base tag or commit to create support branch from (required)")]
        public string BaseBranch { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings) {
        return ExecuteSafe(() => {
            SupportService.Start(settings.Name, settings.BaseBranch);
            WriteSuccess($"Started support branch '{SupportService.Prefix}{settings.Name}' from '{settings.BaseBranch}'");
        });
    }
}
