using System.Text.Json;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Client for querying the NuGet API to check for package updates.
/// </summary>
public sealed class NuGetApiClient : INuGetApiClient, IDisposable {
    private const string _packageId = "karls.gitflow.tool";
    private const string _apiUrl = "https://api.nuget.org/v3-flatcontainer/karls.gitflow.tool/index.json";
    private const int _timeoutSeconds = 5;

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// Creates a new NuGetApiClient with its own HttpClient.
    /// </summary>
    public NuGetApiClient() : this(null) { }

    /// <summary>
    /// Creates a new NuGetApiClient with the specified HttpClient.
    /// </summary>
    /// <param name="httpClient">Optional HttpClient to use. If null, a new one will be created.</param>
    public NuGetApiClient(HttpClient? httpClient) {
        if(httpClient == null) {
            _httpClient = new HttpClient {
                Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
            };
            _ownsHttpClient = true;
        } else {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
    }

    /// <summary>
    /// Gets the latest stable version of the package from NuGet.
    /// Returns null if the query fails or no stable versions are found.
    /// </summary>
    public async Task<Version?> GetLatestVersionAsync(CancellationToken cancellationToken = default) {
        try {
            var response = await _httpClient.GetAsync(_apiUrl, cancellationToken);
            if(!response.IsSuccessStatusCode) {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonDocument.Parse(content);

            if(!json.RootElement.TryGetProperty("versions", out var versionsElement)) {
                return null;
            }

            var versions = new List<Version>();
            foreach(var versionElement in versionsElement.EnumerateArray()) {
                var versionString = versionElement.GetString();
                if(string.IsNullOrEmpty(versionString)) {
                    continue;
                }

                // Only consider stable versions (no prerelease suffixes)
                if(versionString.Contains('-', StringComparison.Ordinal)) {
                    continue;
                }

                if(Version.TryParse(versionString, out var version)) {
                    versions.Add(version);
                }
            }

            // Return the highest version
            return versions.Count > 0 ? versions.Max() : null;
        } catch {
            // Silent failure - network issues, parsing errors, etc.
            return null;
        }
    }

    public void Dispose() {
        if(_ownsHttpClient) {
            _httpClient.Dispose();
        }
    }
}
