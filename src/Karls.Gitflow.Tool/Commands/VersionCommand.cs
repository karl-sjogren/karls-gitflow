using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.Gitflow.Tool.Commands;

/// <summary>
/// Show version information.
/// </summary>
public sealed class VersionCommand : GitFlowCommand<VersionCommand.Settings> {
    public sealed class Settings : CommandSettings {
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";

        Console.MarkupLine($"[blue]git-flow[/] version [yellow]{version}[/]");
        Console.MarkupLine("[dim]A .NET reimplementation of gitflow-avh[/]");

        return 0;
    }
}
