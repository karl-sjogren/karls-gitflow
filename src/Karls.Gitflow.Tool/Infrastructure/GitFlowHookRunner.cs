using System.Diagnostics;
using System.IO.Abstractions;
using Karls.Gitflow.Core;
using Spectre.Console;

namespace Karls.Gitflow.Tool.Infrastructure;

internal sealed class GitFlowHookRunner {
    private readonly IGitService _gitService;
    private readonly IGitExecutor _gitExecutor;
    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;

    public GitFlowHookRunner(IGitService gitService, IGitExecutor gitExecutor, IAnsiConsole console, IFileSystem fileSystem) {
        _gitService = gitService;
        _gitExecutor = gitExecutor;
        _console = console;
        _fileSystem = fileSystem;
    }

    public void Run(
        string command,
        string? action,
        IReadOnlyDictionary<string, string?> parameters,
        IReadOnlySet<string> filterableParameters,
        Action<IReadOnlyDictionary<string, string?>> operation) {
        try {
            RunCore(command, action, parameters, filterableParameters, operation);
        } catch(Exception exception) when(exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) {
            throw new GitFlowException($"Failed to access gitflow hooks: {exception.Message}");
        }
    }

    private void RunCore(
        string command,
        string? action,
        IReadOnlyDictionary<string, string?> parameters,
        IReadOnlySet<string> filterableParameters,
        Action<IReadOnlyDictionary<string, string?>> operation) {
        var values = new Dictionary<string, string?>(parameters, StringComparer.OrdinalIgnoreCase);
        var hookDirectory = GetHookDirectory();

        foreach(var parameter in filterableParameters.OrderBy(parameter => parameter, StringComparer.Ordinal)) {
            if(!values.TryGetValue(parameter, out var value) || value is null) {
                continue;
            }

            var hookName = $"filter-flow-{HookScope(command, action)}-{parameter}";
            var result = RunHook(hookDirectory, hookName, command, action, values, parameter, value);
            if(result is null) {
                continue;
            }

            if(result.ExitCode != 0) {
                throw HookFailed(hookName, result);
            }

            var filteredValue = RemoveTrailingNewline(result.Output);
            if(parameter is not "message" and not "tagmessage"
                && (filteredValue.Contains('\r', StringComparison.Ordinal) || filteredValue.Contains('\n', StringComparison.Ordinal))) {
                throw new GitFlowException($"Gitflow filter hook '{hookName}' must return a single-line value.");
            }

            values[parameter] = filteredValue;
        }

        var arguments = values
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={pair.Value ?? string.Empty}")
            .ToArray();

        var preHookName = $"pre-flow-{HookScope(command, action)}";
        var preResult = RunHook(hookDirectory, preHookName, command, action, values, null, null, arguments);
        if(preResult is not null && preResult.ExitCode != 0) {
            throw HookFailed(preHookName, preResult);
        }

        operation(values);

        var postHookName = $"post-flow-{HookScope(command, action)}";
        var postResult = RunHook(hookDirectory, postHookName, command, action, values, null, null, arguments);
        if(postResult is not null && postResult.ExitCode != 0) {
            throw HookFailed(postHookName, postResult);
        }
    }

    private string GetHookDirectory() {
        if(!_gitService.IsGitRepository()) {
            throw new GitFlowException("Not a git repository.");
        }

        var configuredPath = _gitService.GetConfigValue("gitflow.path.hooks");
        if(!string.IsNullOrWhiteSpace(configuredPath)) {
            var repositoryRoot = _gitService.GetRepositoryRoot();
            return _fileSystem.Path.GetFullPath(configuredPath, repositoryRoot);
        }

        var result = _gitExecutor.Execute(["rev-parse", "--path-format=absolute", "--git-path", "hooks"]);
        if(result.ExitCode != 0 || result.Output.Length == 0) {
            throw new GitFlowException("Unable to determine the Git hooks directory.");
        }

        return result.Output[0];
    }

    private HookResult? RunHook(
        string hookDirectory,
        string hookName,
        string command,
        string? action,
        IReadOnlyDictionary<string, string?> parameters,
        string? filterParameter,
        string? filterValue,
        string[]? operationArguments = null) {
        var hookPath = FindHook(hookDirectory, hookName);
        if(hookPath is null) {
            return null;
        }

        var startInfo = CreateStartInfo(hookPath);
        startInfo.WorkingDirectory = _gitService.GetRepositoryRoot();
        startInfo.Environment["GITFLOW_COMMAND"] = command;
        startInfo.Environment["GITFLOW_ACTION"] = action ?? string.Empty;
        startInfo.Environment["GITFLOW_REPOSITORY"] = startInfo.WorkingDirectory;
        startInfo.Environment["GITFLOW_PARAMETER"] = filterParameter ?? string.Empty;
        startInfo.Environment["GITFLOW_VALUE"] = filterValue ?? string.Empty;

        foreach(var (name, value) in parameters) {
            startInfo.Environment[$"GITFLOW_PARAM_{name.ToUpperInvariant()}"] = value ?? string.Empty;
        }

        if(filterParameter is not null) {
            startInfo.ArgumentList.Add(filterValue ?? string.Empty);
        } else if(operationArguments is not null) {
            foreach(var argument in operationArguments) {
                startInfo.ArgumentList.Add(argument);
            }
        }

        try {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            var output = outputTask.GetAwaiter().GetResult();
            var error = errorTask.GetAwaiter().GetResult();
            if(filterParameter is null) {
                if(output.Length > 0) {
                    _console.WriteLine(Markup.Escape(output.TrimEnd('\r', '\n')));
                }

                if(error.Length > 0) {
                    _console.WriteLine(Markup.Escape(error.TrimEnd('\r', '\n')));
                }
            } else if(error.Length > 0) {
                _console.WriteLine(Markup.Escape(error.TrimEnd('\r', '\n')));
            }

            return new HookResult(process.ExitCode, output, error);
        } catch(Exception exception) when(exception is InvalidOperationException
            or System.ComponentModel.Win32Exception
            or IOException
            or UnauthorizedAccessException
            or ArgumentException) {
            throw new GitFlowException($"Failed to run gitflow hook '{hookName}': {exception.Message}");
        }
    }

    private string? FindHook(string hookDirectory, string hookName) {
        var extensions = new[] { ".sh", ".ps1", ".exe", string.Empty };

        foreach(var extension in extensions) {
            if(extension == ".exe" && !OperatingSystem.IsWindows()) {
                continue;
            }

            if(extension.Length == 0 && OperatingSystem.IsWindows()) {
                continue;
            }

            var path = _fileSystem.Path.Combine(hookDirectory, hookName + extension);
            if(!_fileSystem.File.Exists(path)) {
                continue;
            }

            if(extension is ".sh" or ".ps1" || IsExecutable(path)) {
                return path;
            }
        }

        return null;
    }

    private ProcessStartInfo CreateStartInfo(string hookPath) {
        var startInfo = new ProcessStartInfo {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var extension = _fileSystem.Path.GetExtension(hookPath);
        if(extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase)) {
            startInfo.FileName = GetPowerShellExecutable();
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            if(OperatingSystem.IsWindows() && startInfo.FileName.Equals("powershell.exe", StringComparison.OrdinalIgnoreCase)) {
                startInfo.ArgumentList.Add("-ExecutionPolicy");
                startInfo.ArgumentList.Add("Bypass");
            }

            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(hookPath);
        } else if(extension.Equals(".sh", StringComparison.OrdinalIgnoreCase)) {
            startInfo.FileName = GetShellExecutable();
            startInfo.ArgumentList.Add(hookPath);
        } else {
            startInfo.FileName = hookPath;
        }

        return startInfo;
    }

    private string GetPowerShellExecutable() {
        if(!OperatingSystem.IsWindows()) {
            return "pwsh";
        }

        return FindOnPath("pwsh.exe") ?? "powershell.exe";
    }

    private string GetShellExecutable() {
        if(!OperatingSystem.IsWindows()) {
            return "/bin/sh";
        }

        var shell = FindOnPath("sh.exe");
        if(shell is not null) {
            return shell;
        }

        foreach(var pathEntry in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(_fileSystem.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)) {
            var gitDirectory = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(pathEntry, ".."));
            var gitShell = _fileSystem.Path.Combine(gitDirectory, "usr", "bin", "sh.exe");
            if(_fileSystem.File.Exists(gitShell)) {
                return gitShell;
            }
        }

        return "sh.exe";
    }

    private string? FindOnPath(string executable) {
        foreach(var pathEntry in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(_fileSystem.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)) {
            var candidate = _fileSystem.Path.Combine(pathEntry, executable);
            if(_fileSystem.File.Exists(candidate)) {
                return candidate;
            }
        }

        return null;
    }

    private bool IsExecutable(string path) {
        if(OperatingSystem.IsWindows()) {
            return true;
        }

        var mode = _fileSystem.File.GetUnixFileMode(path);
        const UnixFileMode executableBits = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
        return (mode & executableBits) != 0;
    }

    private static GitFlowException HookFailed(string hookName, HookResult result) {
        var details = result.Error.Trim();
        return new GitFlowException(
            string.IsNullOrEmpty(details)
                ? $"Gitflow hook '{hookName}' failed with exit code {result.ExitCode}."
                : $"Gitflow hook '{hookName}' failed with exit code {result.ExitCode}: {details}");
    }

    private static string HookScope(string command, string? action) {
        return action is null ? command : $"{command}-{action}";
    }

    private static string RemoveTrailingNewline(string value) {
        if(value.EndsWith("\r\n", StringComparison.Ordinal)) {
            return value[..^2];
        }

        return value.EndsWith("\r", StringComparison.Ordinal) || value.EndsWith("\n", StringComparison.Ordinal)
            ? value[..^1]
            : value;
    }

    private sealed record HookResult(int ExitCode, string Output, string Error);
}
