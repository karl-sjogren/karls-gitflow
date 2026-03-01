using Karls.Gitflow.Core;
using Karls.Gitflow.Tool.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class UpdateCheckerIntegrationTests {
    [Fact]
    public async Task NuGetApiClient_CanFetchLatestVersionAsync() {
        // Arrange
        using var client = new NuGetApiClient();

        // Act
        var version = await client.GetLatestVersionAsync(TestContext.Current.CancellationToken);

        // Assert
        version.ShouldNotBeNull();
        version.ShouldBeGreaterThanOrEqualTo(new Version("0.0.7"));
    }

    [Fact]
    public void GitService_CanSetAndGetGlobalConfig() {
        // Arrange
        var gitExecutor = new GitExecutor();
        var gitService = new GitService(gitExecutor);
        var testKey = "gitflow.updatecheck.integrationtest";
        var testValue = "test-value-" + Guid.NewGuid();

        try {
            // Act
            gitService.SetGlobalConfigValue(testKey, testValue);
            var result = gitService.GetGlobalConfigValue(testKey);

            // Assert
            result.ShouldBe(testValue);
        } finally {
            // Cleanup
            try {
                gitExecutor.Execute(["config", "--global", "--unset", testKey]);
            } catch {
                // Ignore cleanup errors
            }
        }
    }
}
