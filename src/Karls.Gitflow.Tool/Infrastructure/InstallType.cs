namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Represents the type of installation for the tool.
/// </summary>
public enum InstallType {
    /// <summary>
    /// Installed as a .NET global tool (via dotnet tool install).
    /// </summary>
    DotNetTool,

    /// <summary>
    /// Installed via the Windows MSI installer.
    /// </summary>
    Msi
}
