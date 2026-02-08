using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class UpdateCheckerTests {
    private readonly IGitService _fakeGitService;
    private readonly INuGetApiClient _fakeNugetClient;
    private readonly IUpdatePromptService _fakePromptService;
    private readonly Version _currentVersion;

    public UpdateCheckerTests() {
        _fakeGitService = A.Fake<IGitService>();
        _fakeNugetClient = A.Fake<INuGetApiClient>();
        _fakePromptService = A.Fake<IUpdatePromptService>();
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
}
