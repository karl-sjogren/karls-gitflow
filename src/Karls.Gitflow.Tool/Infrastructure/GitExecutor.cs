using System.Diagnostics;
using System.IO.Abstractions;
using Karls.Gitflow.Core;

namespace Karls.Gitflow.Tool.Infrastructure;

/// <summary>
/// Executes git commands using the system's git installation.
/// </summary>
public sealed class GitExecutor : IGitExecutor {
    private readonly string _workingDirectory;

    public GitExecutor(IFileSystem? fileSystem = null, string? workingDirectory = null) {
        var fs = fileSystem ?? new FileSystem();
        _workingDirectory = workingDirectory ?? fs.Directory.GetCurrentDirectory();
    }

    public GitExecutorResult Execute(string command) {
        using var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = _workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        var outputLines = new List<string>();
        var errorLines = new List<string>();

        process.OutputDataReceived += (sender, e) => {
            if(e.Data != null) {
                outputLines.Add(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) => {
            if(e.Data != null) {
                errorLines.Add(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        // Git sometimes writes informational messages to stderr even on success
        // Only include error lines in output if the command failed
        var output = process.ExitCode == 0
            ? outputLines.ToArray()
            : outputLines.Concat(errorLines).ToArray();

        return new GitExecutorResult(output, process.ExitCode);
    }
}
