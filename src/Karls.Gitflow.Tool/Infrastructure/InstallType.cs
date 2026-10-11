namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Represents the type of installation for the tool.
/// </summary>
public enum InstallType {
    /// <summary>
    /// The installation type could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// Installed as a .NET global tool (via dotnet tool install).
    /// </summary>
    DotNetTool,

    /// <summary>
    /// Installed via the Windows MSI installer.
    /// </summary>
    Msi,

    /// <summary>
    /// Installed via the macOS installer package.
    /// </summary>
    Pkg,

    /// <summary>
    /// Installed via Homebrew (macOS or Linux).
    /// </summary>
    Homebrew
}
