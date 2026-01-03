using Spectre.Console;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Provides thread-local context for gitflow operations.
/// Used primarily for testing to enable parallel test execution by avoiding global state.
/// </summary>
public static class GitFlowContext {
    private static readonly AsyncLocal<string?> _workingDirectory = new();
    private static readonly AsyncLocal<IAnsiConsole?> _console = new();

    /// <summary>
    /// Gets or sets the working directory for git operations.
    /// If null, the current directory is used.
    /// </summary>
    public static string? WorkingDirectory {
        get => _workingDirectory.Value;
        set => _workingDirectory.Value = value;
    }

    /// <summary>
    /// Gets or sets the console for output.
    /// If null, AnsiConsole.Console is used.
    /// </summary>
    public static IAnsiConsole? Console {
        get => _console.Value;
        set => _console.Value = value;
    }

    /// <summary>
    /// Gets the effective console (context console or default AnsiConsole).
    /// </summary>
    public static IAnsiConsole EffectiveConsole => Console ?? AnsiConsole.Console;

    /// <summary>
    /// Resets the context to default values.
    /// </summary>
    public static void Reset() {
        WorkingDirectory = null;
        Console = null;
    }
}
