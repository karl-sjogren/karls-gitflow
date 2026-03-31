using System.IO.Abstractions;
using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class UpdateCheckerTests {
    private readonly IGitService _fakeGitService;
    private readonly INuGetApiClient _fakeNugetClient;
    private readonly IUpdatePromptService _fakePromptService;
    private readonly IFileSystem _fakeFileSystem;
    private readonly Version _currentVersion;

    public UpdateCheckerTests() {
        _fakeGitService = A.Fake<IGitService>();
        _fakeNugetClient = A.Fake<INuGetApiClient>();
        _fakePromptService = A.Fake<IUpdatePromptService>();
        _fakeFileSystem = A.Fake<IFileSystem>();
        _currentVersion = new Version("0.0.7");
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenDisabled_ReturnsFalseAsync() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetGlobalConfigValue("gitflow.updatecheck.enabled"))
            .Returns("false");

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion);

        // Act
        var result = await sut.CheckForUpdatesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
        A.CallTo(() => _fakeNugetClient.GetLatestVersionAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenFirstRun_SetsConfigAndReturnsFalseAsync() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetGlobalConfigValue("gitflow.updatecheck.enabled"))
            .Returns(null);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion);

        // Act
        var result = await sut.CheckForUpdatesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
        A.CallTo(() => _fakeGitService.SetGlobalConfigValue("gitflow.updatecheck.enabled", "true"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeGitService.SetGlobalConfigValue("gitflow.updatecheck.lastcheck", A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeNugetClient.GetLatestVersionAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenExceptionThrown_ReturnsFalseAsync() {
        // Arrange
        A.CallTo(() => _fakeGitService.GetGlobalConfigValue(A<string>._))
            .Throws<Exception>();

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion);

        // Act
        var result = await sut.CheckForUpdatesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenUpdateAvailableAndUserSelectsUpdateNow_CallsDisplayUpdateInstructionsAsync() {
        // Arrange
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var lastCheck = fakeTime.GetUtcNow().AddDays(-30).ToString("o");

        A.CallTo(() => _fakeGitService.GetGlobalConfigValue("gitflow.updatecheck.enabled"))
            .Returns("true");
        A.CallTo(() => _fakeGitService.GetGlobalConfigValue("gitflow.updatecheck.lastcheck"))
            .Returns(lastCheck);
        A.CallTo(() => _fakeNugetClient.GetLatestVersionAsync(A<CancellationToken>._))
            .Returns(new Version("1.0.0"));
        A.CallTo(() => _fakePromptService.PromptUser(A<Version>._, A<Version>._))
            .Returns(UpdatePromptResult.UpdateNow);
        A.CallTo(() => _fakeFileSystem.Path.Combine(A<string>._, A<string>._, A<string>._))
            .Returns("/home/user/.dotnet/tools");

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fakeTime, _fakeFileSystem);

        // Act
        var result = await sut.CheckForUpdatesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeTrue();
        A.CallTo(() => _fakePromptService.DisplayUpdateInstructions(A<InstallType>._))
            .MustHaveHappenedOnceExactly();
    }

    #region DetectInstallType

    [Fact]
    public void DetectInstallType_WhenProcessPathIsInDotNetToolsDirectory_ReturnsDotNetTool() {
        // Arrange
        var userProfilePath = "/home/testuser";
        var dotnetToolsPath = "/home/testuser/.dotnet/tools";

        A.CallTo(() => _fakeFileSystem.Path.Combine(userProfilePath, ".dotnet", "tools"))
            .Returns(dotnetToolsPath);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType($"{dotnetToolsPath}/git-flow", userProfilePath);

        // Assert
        result.ShouldBe(InstallType.DotNetTool);
    }

    [Fact]
    public void DetectInstallType_WhenProcessPathIsOutsideDotNetToolsDirectory_ReturnsMsi() {
        // Arrange
        var userProfilePath = "/home/testuser";
        var dotnetToolsPath = "/home/testuser/.dotnet/tools";

        A.CallTo(() => _fakeFileSystem.Path.Combine(userProfilePath, ".dotnet", "tools"))
            .Returns(dotnetToolsPath);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType(@"C:\Program Files\Karls Gitflow\git-flow.exe", userProfilePath);

        // Assert
        result.ShouldBe(InstallType.Msi);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DetectInstallType_WhenProcessPathIsNullOrEmpty_ReturnsDotNetTool(string? processPath) {
        // Arrange
        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType(processPath, "/home/testuser");

        // Assert
        result.ShouldBe(InstallType.DotNetTool);
    }

    [Fact]
    public void DetectInstallType_WhenProcessPathIsInSimilarlyNamedDirectory_ReturnsMsi() {
        // Arrange - ".dotnet/tools-backup" must not match ".dotnet/tools"
        var userProfilePath = "/home/testuser";
        var dotnetToolsPath = "/home/testuser/.dotnet/tools";

        A.CallTo(() => _fakeFileSystem.Path.Combine(userProfilePath, ".dotnet", "tools"))
            .Returns(dotnetToolsPath);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType("/home/testuser/.dotnet/tools-backup/git-flow", userProfilePath);

        // Assert
        result.ShouldBe(InstallType.Msi);
    }

    [Theory]
    [InlineData("/opt/homebrew/bin/git-flow")]
    [InlineData("/opt/homebrew/Cellar/karls-gitflow/1.0.0/bin/git-flow")]
    [InlineData("/usr/local/Homebrew/bin/git-flow")]
    public void DetectInstallType_WhenProcessPathIsInHomebrewDirectory_ReturnsHomebrew(string processPath) {
        // Arrange
        var userProfilePath = "/home/testuser";
        var dotnetToolsPath = "/home/testuser/.dotnet/tools";

        A.CallTo(() => _fakeFileSystem.Path.Combine(userProfilePath, ".dotnet", "tools"))
            .Returns(dotnetToolsPath);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType(processPath, userProfilePath);

        // Assert
        result.ShouldBe(InstallType.Homebrew);
    }

    [Fact]
    public void DetectInstallType_WhenProcessPathIsInLinuxbrewDirectory_ReturnsHomebrew() {
        // Arrange
        var userProfilePath = "/home/testuser";
        var dotnetToolsPath = "/home/testuser/.dotnet/tools";

        A.CallTo(() => _fakeFileSystem.Path.Combine(userProfilePath, ".dotnet", "tools"))
            .Returns(dotnetToolsPath);

        var sut = new UpdateChecker(_fakeGitService, _fakeNugetClient, _fakePromptService, _currentVersion, fileSystem: _fakeFileSystem);

        // Act
        var result = sut.DetectInstallType("/home/linuxbrew/.linuxbrew/bin/git-flow", userProfilePath);

        // Assert
        result.ShouldBe(InstallType.Homebrew);
    }

    #endregion
}
