namespace Karls.Gitflow.Core.Tests;

public sealed class GitExceptionTests {
    [Fact]
    public void DefaultConstructor_CreatesException() {
        // Act
        var ex = new GitException();

        // Assert
        ex.ShouldNotBeNull();
    }

    [Fact]
    public void MessageConstructor_SetsMessage() {
        // Act
        var ex = new GitException("test message");

        // Assert
        ex.Message.ShouldBe("test message");
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_SetsMessageAndInnerException() {
        // Arrange
        var innerException = new InvalidOperationException("inner");

        // Act
        var ex = new GitException("outer message", innerException);

        // Assert
        ex.Message.ShouldBe("outer message");
        ex.InnerException.ShouldBe(innerException);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_WithNullInnerException_CreatesException() {
        // Act
        var ex = new GitException("message", null);

        // Assert
        ex.Message.ShouldBe("message");
        ex.InnerException.ShouldBeNull();
    }
}
