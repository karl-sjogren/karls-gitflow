namespace Karls.Gitflow.Core.Tests;

public class GitFlowConfigurationTests {
    [Fact]
    public void Default_ReturnsValidConfiguration() {
        // Act
        var config = GitFlowConfiguration.Default;

        // Assert
        config.MainBranch.ShouldBe("main");
        config.DevelopBranch.ShouldBe("develop");
        config.FeaturePrefix.ShouldBe("feature/");
        config.BugfixPrefix.ShouldBe("bugfix/");
        config.ReleasePrefix.ShouldBe("release/");
        config.HotfixPrefix.ShouldBe("hotfix/");
        config.SupportPrefix.ShouldBe("support/");
        config.VersionTagPrefix.ShouldBe("");
    }

    [Fact]
    public void IsValid_WithAllRequiredFields_ReturnsTrue() {
        // Arrange
        var config = GitFlowConfiguration.Default;

        // Act & Assert
        config.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "develop", "feature/", "bugfix/", "release/", "hotfix/", "support/")]
    [InlineData("main", "", "feature/", "bugfix/", "release/", "hotfix/", "support/")]
    [InlineData("main", "develop", "", "bugfix/", "release/", "hotfix/", "support/")]
    [InlineData("main", "develop", "feature/", "", "release/", "hotfix/", "support/")]
    [InlineData("main", "develop", "feature/", "bugfix/", "", "hotfix/", "support/")]
    [InlineData("main", "develop", "feature/", "bugfix/", "release/", "", "support/")]
    [InlineData("main", "develop", "feature/", "bugfix/", "release/", "hotfix/", "")]
    public void IsValid_WithMissingRequiredField_ReturnsFalse(
        string mainBranch,
        string developBranch,
        string featurePrefix,
        string bugfixPrefix,
        string releasePrefix,
        string hotfixPrefix,
        string supportPrefix) {
        // Arrange
        var config = new GitFlowConfiguration {
            MainBranch = mainBranch,
            DevelopBranch = developBranch,
            FeaturePrefix = featurePrefix,
            BugfixPrefix = bugfixPrefix,
            ReleasePrefix = releasePrefix,
            HotfixPrefix = hotfixPrefix,
            SupportPrefix = supportPrefix,
            VersionTagPrefix = ""
        };

        // Act & Assert
        config.IsValid().ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyVersionTagPrefix_ReturnsTrue() {
        // Arrange - VersionTagPrefix can be empty
        var config = GitFlowConfiguration.Default with { VersionTagPrefix = "" };

        // Act & Assert
        config.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithCustomVersionTagPrefix_ReturnsTrue() {
        // Arrange
        var config = GitFlowConfiguration.Default with { VersionTagPrefix = "v" };

        // Act & Assert
        config.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void DefaultValues_ContainsExpectedConstants() {
        // Assert
        GitFlowConfiguration.DefaultValues.MainBranch.ShouldBe("main");
        GitFlowConfiguration.DefaultValues.DevelopBranch.ShouldBe("develop");
        GitFlowConfiguration.DefaultValues.FeaturePrefix.ShouldBe("feature/");
        GitFlowConfiguration.DefaultValues.BugfixPrefix.ShouldBe("bugfix/");
        GitFlowConfiguration.DefaultValues.ReleasePrefix.ShouldBe("release/");
        GitFlowConfiguration.DefaultValues.HotfixPrefix.ShouldBe("hotfix/");
        GitFlowConfiguration.DefaultValues.SupportPrefix.ShouldBe("support/");
        GitFlowConfiguration.DefaultValues.VersionTagPrefix.ShouldBe("");
    }
}
