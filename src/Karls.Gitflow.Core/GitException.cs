namespace Karls.Gitflow.Core;

public class GitException : Exception {
    public GitException() {
    }

    public GitException(string message) : base(message) {
    }

    public GitException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
