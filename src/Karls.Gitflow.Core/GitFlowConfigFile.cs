using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Karls.Gitflow.Core;

/// <summary>
/// Handles reading and writing the <c>.gitflow</c> configuration file that can be committed
/// to a repository to share settings across a team.
/// </summary>
public sealed partial class GitFlowConfigFile {
    /// <summary>
    /// The name of the configuration file.
    /// </summary>
    public const string FileName = ".gitflow";

    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(GitFlowConfigFileDto))]
    private sealed partial class GitFlowConfigFileSerializerContext : JsonSerializerContext { }

    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="GitFlowConfigFile"/>.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use. When <c>null</c>, the real file system is used.</param>
    public GitFlowConfigFile(IFileSystem? fileSystem = null) {
        _fileSystem = fileSystem ?? new FileSystem();
    }

    /// <summary>
    /// Checks whether a <c>.gitflow</c> file exists in the given repository root.
    /// </summary>
    public bool Exists(string repositoryRoot) {
        var path = _fileSystem.Path.Combine(repositoryRoot, FileName);
        return _fileSystem.File.Exists(path);
    }

    /// <summary>
    /// Loads the <c>.gitflow</c> configuration file from the given repository root.
    /// Returns <c>null</c> if the file does not exist or cannot be parsed.
    /// </summary>
    public GitFlowConfiguration? Load(string repositoryRoot) {
        var path = _fileSystem.Path.Combine(repositoryRoot, FileName);
        if(!_fileSystem.File.Exists(path)) {
            return null;
        }

        try {
            var json = _fileSystem.File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize(json, GitFlowConfigFileSerializerContext.Default.GitFlowConfigFileDto);
            return dto?.ToConfiguration();
        } catch(JsonException) {
            return null;
        }
    }

    /// <summary>
    /// Saves the given gitflow configuration to a <c>.gitflow</c> file in the repository root.
    /// </summary>
    public void Save(string repositoryRoot, GitFlowConfiguration config) {
        var path = _fileSystem.Path.Combine(repositoryRoot, FileName);
        var dto = GitFlowConfigFileDto.FromConfiguration(config);
        var json = JsonSerializer.Serialize(dto, GitFlowConfigFileSerializerContext.Default.GitFlowConfigFileDto);
        _fileSystem.File.WriteAllText(path, json);
    }

    private sealed class GitFlowConfigFileDto {
        public string? MainBranch { get; set; }
        public string? DevelopBranch { get; set; }
        public string? FeaturePrefix { get; set; }
        public string? BugfixPrefix { get; set; }
        public string? ReleasePrefix { get; set; }
        public string? HotfixPrefix { get; set; }
        public string? SupportPrefix { get; set; }
        public string? VersionTagPrefix { get; set; }
        public string? TagMessageTemplate { get; set; }

        public GitFlowConfiguration? ToConfiguration() {
            if(string.IsNullOrWhiteSpace(MainBranch) ||
               string.IsNullOrWhiteSpace(DevelopBranch) ||
               string.IsNullOrWhiteSpace(FeaturePrefix) ||
               string.IsNullOrWhiteSpace(BugfixPrefix) ||
               string.IsNullOrWhiteSpace(ReleasePrefix) ||
               string.IsNullOrWhiteSpace(HotfixPrefix) ||
               string.IsNullOrWhiteSpace(SupportPrefix)) {
                return null;
            }

            return new GitFlowConfiguration {
                MainBranch = MainBranch,
                DevelopBranch = DevelopBranch,
                FeaturePrefix = FeaturePrefix,
                BugfixPrefix = BugfixPrefix,
                ReleasePrefix = ReleasePrefix,
                HotfixPrefix = HotfixPrefix,
                SupportPrefix = SupportPrefix,
                VersionTagPrefix = VersionTagPrefix ?? string.Empty,
                TagMessageTemplate = TagMessageTemplate ?? string.Empty
            };
        }

        public static GitFlowConfigFileDto FromConfiguration(GitFlowConfiguration config) {
            return new GitFlowConfigFileDto {
                MainBranch = config.MainBranch,
                DevelopBranch = config.DevelopBranch,
                FeaturePrefix = config.FeaturePrefix,
                BugfixPrefix = config.BugfixPrefix,
                ReleasePrefix = config.ReleasePrefix,
                HotfixPrefix = config.HotfixPrefix,
                SupportPrefix = config.SupportPrefix,
                VersionTagPrefix = string.IsNullOrEmpty(config.VersionTagPrefix) ? null : config.VersionTagPrefix,
                TagMessageTemplate = string.IsNullOrEmpty(config.TagMessageTemplate) ? null : config.TagMessageTemplate
            };
        }
    }
}
