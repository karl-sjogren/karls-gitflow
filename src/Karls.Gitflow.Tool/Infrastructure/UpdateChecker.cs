using System.IO.Abstractions;
using Karls.Gitflow.Core;
using System.Globalization;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Orchestrates update checking for the tool.
/// </summary>
public sealed class UpdateChecker {
    private const string _enabledConfigKey = "gitflow.updatecheck.enabled";
    private const string _lastCheckConfigKey = "gitflow.updatecheck.lastcheck";
    private const int _checkIntervalDays = 14;

    private readonly IGitService _gitService;
    private readonly Func<INuGetApiClient> _nugetClientFactory;
    private readonly IUpdatePromptService _promptService;
    private readonly Version _currentVersion;
    private readonly TimeProvider _timeProvider;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Creates a new UpdateChecker with a lazy NuGet client factory.
    /// </summary>
    /// <param name="gitService">The git service for reading/writing config.</param>
    /// <param name="nugetClientFactory">Factory to create the NuGet API client only when needed.</param>
    /// <param name="promptService">The prompt service for user interaction.</param>
    /// <param name="currentVersion">The current version of the tool.</param>
    /// <param name="timeProvider">The time provider for getting current time (optional, defaults to system time).</param>
    /// <param name="fileSystem">The file system abstraction (optional, defaults to real file system).</param>
    public UpdateChecker(
            IGitService gitService,
            Func<INuGetApiClient> nugetClientFactory,
            IUpdatePromptService promptService,
            Version currentVersion,
            TimeProvider? timeProvider = null,
            IFileSystem? fileSystem = null) {
        _gitService = gitService;
        _nugetClientFactory = nugetClientFactory;
        _promptService = promptService;
        _currentVersion = currentVersion;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _fileSystem = fileSystem ?? new FileSystem();
    }

    /// <summary>
    /// Checks for updates and prompts the user if an update is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the application should exit (user chose to update), false otherwise.</returns>
    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default) {
        var result = await CheckForUpdatesCoreAsync(forceCheck: false, cancellationToken);
        return result == UpdateCheckResult.UpdateNow;
    }

    /// <summary>
    /// Checks for updates immediately, regardless of the automatic check interval or preference.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the update check.</returns>
    public Task<UpdateCheckResult> ForceCheckForUpdatesAsync(CancellationToken cancellationToken = default) {
        return CheckForUpdatesCoreAsync(forceCheck: true, cancellationToken);
    }

    private async Task<UpdateCheckResult> CheckForUpdatesCoreAsync(bool forceCheck, CancellationToken cancellationToken) {
        try {
            // Check if update checks are enabled
            var enabled = _gitService.GetGlobalConfigValue(_enabledConfigKey);
            if(!forceCheck && enabled == "false") {
                return UpdateCheckResult.Skipped; // User disabled update checks
            }

            // Check if this is the first run
            if(!forceCheck && enabled == null) {
                // First run: enable by default and set lastcheck to now, but don't check yet
                _gitService.SetGlobalConfigValue(_enabledConfigKey, "true");
                _gitService.SetGlobalConfigValue(_lastCheckConfigKey, _timeProvider.GetUtcNow().ToString("o"));
                return UpdateCheckResult.Skipped;
            }

            // Check if enough time has passed since last check
            if(!forceCheck) {
                var lastCheckStr = _gitService.GetGlobalConfigValue(_lastCheckConfigKey);
                if(lastCheckStr != null) {
                    if(DateTimeOffset.TryParse(lastCheckStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastCheck)) {
                        var daysSinceLastCheck = (_timeProvider.GetUtcNow() - lastCheck).TotalDays;
                        if(daysSinceLastCheck < _checkIntervalDays) {
                            return UpdateCheckResult.Skipped; // Not time to check yet
                        }
                    }
                }
            }

            // Fetch latest version from NuGet
            var nugetClient = _nugetClientFactory();
            var latestVersion = await nugetClient.GetLatestVersionAsync(cancellationToken);

            // Update last check time
            _gitService.SetGlobalConfigValue(_lastCheckConfigKey, _timeProvider.GetUtcNow().ToString("o"));

            if(latestVersion == null) {
                return UpdateCheckResult.Failed; // Failed to fetch
            }

            // Compare versions
            if(latestVersion <= _currentVersion) {
                return UpdateCheckResult.UpToDate; // Already on latest or newer version
            }

            // Prompt user
            var result = _promptService.PromptUser(_currentVersion, latestVersion);

            var installType = DetectInstallType();

            switch(result) {
                case UpdatePromptResult.UpdateNow:
                    _promptService.DisplayUpdateInstructions(installType);
                    return UpdateCheckResult.UpdateNow;

                case UpdatePromptResult.RemindLater:
                    // lastcheck is already updated above
                    return UpdateCheckResult.UpdateAvailable;

                case UpdatePromptResult.DontAskAgain:
                    _gitService.SetGlobalConfigValue(_enabledConfigKey, "false");
                    return UpdateCheckResult.UpdateAvailable;

                default:
                    return UpdateCheckResult.UpdateAvailable;
            }
        } catch {
            // Silent failure - never interrupt the workflow
            return UpdateCheckResult.Failed;
        }
    }

    /// <summary>
    /// Detects the install type based on the current process path.
    /// </summary>
    /// <returns>The detected install type.</returns>
    internal InstallType DetectInstallType() {
        return DetectInstallType(
            Environment.ProcessPath,
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    /// <summary>
    /// Detects the install type based on the provided process path and user profile path.
    /// </summary>
    /// <param name="processPath">The path to the running process executable.</param>
    /// <param name="userProfilePath">The user profile directory path.</param>
    /// <returns>The detected install type.</returns>
    internal InstallType DetectInstallType(string? processPath, string userProfilePath) {
        return DetectInstallType(processPath, userProfilePath, OperatingSystem.IsWindows(), OperatingSystem.IsMacOS());
    }

    internal InstallType DetectInstallType(string? processPath, string userProfilePath, bool isWindows, bool isMacOS) {
        if(string.IsNullOrEmpty(processPath)) {
            return InstallType.DotNetTool; // Default to dotnet tool if path is unavailable
        }

        var dotnetToolsPath = _fileSystem.Path.Combine(userProfilePath, ".dotnet", "tools");

        if(processPath.StartsWith(dotnetToolsPath, StringComparison.OrdinalIgnoreCase) &&
           (processPath.Length == dotnetToolsPath.Length || processPath[dotnetToolsPath.Length] is '/' or '\\')) {
            return InstallType.DotNetTool;
        }

        if(processPath.Contains("/homebrew/", StringComparison.OrdinalIgnoreCase) ||
           processPath.Contains("/linuxbrew/", StringComparison.OrdinalIgnoreCase) ||
           processPath.Contains("/.linuxbrew/", StringComparison.OrdinalIgnoreCase)) {
            return InstallType.Homebrew;
        }

        if(isMacOS && string.Equals(processPath, "/usr/local/bin/git-flow", StringComparison.OrdinalIgnoreCase)) {
            return InstallType.Pkg;
        }

        return isWindows ? InstallType.Msi : InstallType.Unknown;
    }
}
