namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Interface for querying the NuGet API to check for package updates.
/// </summary>
public interface INuGetApiClient {
    /// <summary>
    /// Gets the latest stable version of the package from NuGet.
    /// Returns null if the query fails or no stable versions are found.
    /// </summary>
    Task<Version?> GetLatestVersionAsync(CancellationToken cancellationToken = default);
}
