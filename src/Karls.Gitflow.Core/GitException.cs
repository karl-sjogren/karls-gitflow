namespace Karls.Gitflow.Core;

/// <summary>
/// Exception thrown when a git command fails.
/// </summary>
public sealed class GitException : Exception {
    public GitException() {
    }

    public GitException(string message) : base(message) {
    }

    public GitException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
