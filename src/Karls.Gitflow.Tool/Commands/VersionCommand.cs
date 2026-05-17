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

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version;
        var version = assemblyVersion != null
            ? $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}"
            : "0.0.0";

        Console.MarkupLine($"[blue]git flow[/] version [yellow]{version}[/]");
        Console.MarkupLine("[dim]A .NET reimplementation of gitflow-avh[/]");

        return 0;
    }
}
