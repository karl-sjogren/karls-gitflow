using System.IO.Abstractions.TestingHelpers;

namespace Karls.Gitflow.Core.Tests;

public sealed class GitFlowConfigFileTests {
    private readonly MockFileSystem _fileSystem;
    private readonly GitFlowConfigFile _sut;

    public GitFlowConfigFileTests() {
        _fileSystem = new MockFileSystem();
        _sut = new GitFlowConfigFile(_fileSystem);
    }

    #region Exists

    [Fact]
    public void Exists_WhenFileExists_ReturnsTrue() {
        // Arrange
        _fileSystem.AddFile("/repo/.gitflow", new MockFileData("{}"));

        // Act
        var result = _sut.Exists("/repo");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Exists_WhenFileDoesNotExist_ReturnsFalse() {
        // Act
        var result = _sut.Exists("/repo");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Load

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsNull() {
        // Act
        var result = _sut.Load("/repo");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Load_WhenFileContainsInvalidJson_ReturnsNull() {
        // Arrange
        _fileSystem.AddFile("/repo/.gitflow", new MockFileData("not valid json {{{"));

        // Act
        var result = _sut.Load("/repo");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Load_WhenFileContainsValidConfiguration_ReturnsConfiguration() {
        // Arrange
        _fileSystem.AddFile("/repo/.gitflow", new MockFileData("""
            {
              "mainBranch": "main",
              "developBranch": "develop",
              "featurePrefix": "feature/",
              "bugfixPrefix": "bugfix/",
              "releasePrefix": "release/",
              "hotfixPrefix": "hotfix/",
              "supportPrefix": "support/",
              "versionTagPrefix": "v",
              "tagMessageTemplate": "Release {version}"
            }
            """));

        // Act
        var result = _sut.Load("/repo");

        // Assert
        result.ShouldNotBeNull();
        result.MainBranch.ShouldBe("main");
        result.DevelopBranch.ShouldBe("develop");
        result.FeaturePrefix.ShouldBe("feature/");
        result.BugfixPrefix.ShouldBe("bugfix/");
        result.ReleasePrefix.ShouldBe("release/");
        result.HotfixPrefix.ShouldBe("hotfix/");
        result.SupportPrefix.ShouldBe("support/");
        result.VersionTagPrefix.ShouldBe("v");
        result.TagMessageTemplate.ShouldBe("Release {version}");
    }

    [Fact]
    public void Load_WhenFileContainsConfigurationWithoutOptionalFields_ReturnsConfiguration() {
        // Arrange - VersionTagPrefix and TagMessageTemplate are optional
        _fileSystem.AddFile("/repo/.gitflow", new MockFileData("""
            {
              "mainBranch": "main",
              "developBranch": "develop",
              "featurePrefix": "feature/",
              "bugfixPrefix": "bugfix/",
              "releasePrefix": "release/",
              "hotfixPrefix": "hotfix/",
              "supportPrefix": "support/"
            }
            """));

        // Act
        var result = _sut.Load("/repo");

        // Assert
        result.ShouldNotBeNull();
        result.VersionTagPrefix.ShouldBe(string.Empty);
        result.TagMessageTemplate.ShouldBe(string.Empty);
    }

    [Fact]
    public void Load_WhenFileContainsConfigurationWithMissingRequiredFields_ReturnsNull() {
        // Arrange - Missing required fields (no supportPrefix)
        _fileSystem.AddFile("/repo/.gitflow", new MockFileData("""
            {
              "mainBranch": "main",
              "developBranch": "develop",
              "featurePrefix": "feature/"
            }
            """));

        // Act
        var result = _sut.Load("/repo");

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region Save

    [Fact]
    public void Save_WritesConfigurationToFile() {
        // Arrange
        _fileSystem.AddDirectory("/repo");
        var config = GitFlowConfiguration.Default with {
            VersionTagPrefix = "v",
            TagMessageTemplate = "Release {version}"
        };

        // Act
        _sut.Save("/repo", config);

        // Assert
        _fileSystem.FileExists("/repo/.gitflow").ShouldBeTrue();
        var content = _fileSystem.File.ReadAllText("/repo/.gitflow");
        content.ShouldContain("\"mainBranch\": \"main\"");
        content.ShouldContain("\"developBranch\": \"develop\"");
        content.ShouldContain("\"versionTagPrefix\": \"v\"");
        content.ShouldContain("\"tagMessageTemplate\": \"Release {version}\"");
    }

    [Fact]
    public void Save_WhenVersionTagPrefixIsEmpty_OmitsItFromOutput() {
        // Arrange
        _fileSystem.AddDirectory("/repo");
        var config = GitFlowConfiguration.Default with { VersionTagPrefix = "" };

        // Act
        _sut.Save("/repo", config);

        // Assert
        var content = _fileSystem.File.ReadAllText("/repo/.gitflow");
        content.ShouldNotContain("versionTagPrefix");
    }

    [Fact]
    public void Save_RoundTrip_LoadsOriginalConfiguration() {
        // Arrange
        _fileSystem.AddDirectory("/repo");
        var originalConfig = GitFlowConfiguration.Default with {
            VersionTagPrefix = "v",
            TagMessageTemplate = "Version {version}"
        };

        // Act
        _sut.Save("/repo", originalConfig);
        var loadedConfig = _sut.Load("/repo");

        // Assert
        loadedConfig.ShouldNotBeNull();
        loadedConfig.MainBranch.ShouldBe(originalConfig.MainBranch);
        loadedConfig.DevelopBranch.ShouldBe(originalConfig.DevelopBranch);
        loadedConfig.FeaturePrefix.ShouldBe(originalConfig.FeaturePrefix);
        loadedConfig.BugfixPrefix.ShouldBe(originalConfig.BugfixPrefix);
        loadedConfig.ReleasePrefix.ShouldBe(originalConfig.ReleasePrefix);
        loadedConfig.HotfixPrefix.ShouldBe(originalConfig.HotfixPrefix);
        loadedConfig.SupportPrefix.ShouldBe(originalConfig.SupportPrefix);
        loadedConfig.VersionTagPrefix.ShouldBe(originalConfig.VersionTagPrefix);
        loadedConfig.TagMessageTemplate.ShouldBe(originalConfig.TagMessageTemplate);
    }

    #endregion
}
