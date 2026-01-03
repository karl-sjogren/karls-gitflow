namespace Karls.Gitflow.Core;

/// <summary>
/// Exception thrown when a gitflow operation fails.
/// </summary>
public class GitFlowException : Exception {
    public GitFlowException() {
    }

    public GitFlowException(string message) : base(message) {
    }

    public GitFlowException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
