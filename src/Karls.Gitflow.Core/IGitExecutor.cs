namespace Karls.Gitflow.Core;

/// <summary>
/// Abstraction for executing git commands.
/// </summary>
public interface IGitExecutor {
    GitExecutorResult Execute(string[] args, bool captureOutput = true);
}

/// <summary>
/// Result of executing a git command.
/// </summary>
/// <param name="Output">The output lines from the command (stdout). Empty when output is not captured.</param>
/// <param name="ExitCode">The exit code of the command.</param>
/// <param name="Messages">Informational messages from the command (stderr). These may include
/// server responses like PR creation links or security warnings. Empty when output is not captured.</param>
public sealed record GitExecutorResult(string[] Output, int ExitCode, string[]? Messages = null) {
    /// <summary>
    /// Gets the messages, or an empty array if null.
    /// </summary>
    public string[] Messages { get; } = Messages ?? [];
}
