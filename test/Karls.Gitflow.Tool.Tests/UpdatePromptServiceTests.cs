using Karls.Gitflow.Tool.Infrastructure;
using Spectre.Console.Testing;

namespace Karls.Gitflow.Tool.Tests;

public class UpdatePromptServiceTests {
    #region DisplayUpdateInstructions

    [Fact]
    public void DisplayUpdateInstructions_WithDotNetTool_ShowsDotNetUpdateCommand() {
        // Arrange
        var console = new TestConsole();
        var sut = new UpdatePromptService(console);

        // Act
        sut.DisplayUpdateInstructions(InstallType.DotNetTool);

        // Assert
        console.Output.ShouldContain("dotnet tool update -g Karls.Gitflow.Tool");
    }

    [Fact]
    public void DisplayUpdateInstructions_WithMsi_ShowsReleasesLink() {
        // Arrange
        var console = new TestConsole();
        var sut = new UpdatePromptService(console);

        // Act
        sut.DisplayUpdateInstructions(InstallType.Msi);

        // Assert
        console.Output.ShouldContain("https://github.com/karl-sjogren/karls-gitflow/releases/latest");
    }

    [Fact]
    public void DisplayUpdateInstructions_WithHomebrew_ShowsBrewUpgradeCommand() {
        // Arrange
        var console = new TestConsole();
        var sut = new UpdatePromptService(console);

        // Act
        sut.DisplayUpdateInstructions(InstallType.Homebrew);

        // Assert
        console.Output.ShouldContain("brew upgrade karl-sjogren/tap/karls-gitflow");
    }

    #endregion

    #region PromptUser

    [Fact]
    public void PromptUser_WhenUserSelectsUpdateNow_ReturnsUpdateNow() {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        // "Yes, update now" is the first item - just press Enter to select it
        console.Input.PushKey(ConsoleKey.Enter);
        var sut = new UpdatePromptService(console);

        // Act
        var result = sut.PromptUser(new Version("0.0.1"), new Version("1.0.0"));

        // Assert
        result.ShouldBe(UpdatePromptResult.UpdateNow);
    }

    [Fact]
    public void PromptUser_WhenUserSelectsRemindLater_ReturnsRemindLater() {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        // "No, remind me later" is the second item - press Down then Enter
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        var sut = new UpdatePromptService(console);

        // Act
        var result = sut.PromptUser(new Version("0.0.1"), new Version("1.0.0"));

        // Assert
        result.ShouldBe(UpdatePromptResult.RemindLater);
    }

    [Fact]
    public void PromptUser_WhenUserSelectsDontAskAgain_ReturnsDontAskAgain() {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        // "Don't ask again" is the third item - press Down twice then Enter
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        var sut = new UpdatePromptService(console);

        // Act
        var result = sut.PromptUser(new Version("0.0.1"), new Version("1.0.0"));

        // Assert
        result.ShouldBe(UpdatePromptResult.DontAskAgain);
    }

    [Fact]
    public void PromptUser_DisplaysCurrentAndLatestVersion() {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.Enter);
        var sut = new UpdatePromptService(console);
        var current = new Version("0.1.2");
        var latest = new Version("1.2.3");

        // Act
        sut.PromptUser(current, latest);

        // Assert
        console.Output.ShouldContain("0.1.2");
        console.Output.ShouldContain("1.2.3");
    }

    #endregion
}
