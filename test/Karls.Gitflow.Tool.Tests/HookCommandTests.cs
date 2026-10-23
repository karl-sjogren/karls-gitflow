using System.ComponentModel;
using System.Diagnostics;
using Karls.Gitflow.Tool.Tests.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public sealed class HookCommandTests : IDisposable {
    private readonly GitRepositoryFixture _repo = new();

    public HookCommandTests() {
        _repo.InitializeGitFlow();
    }

    public void Dispose() {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void PreHook_NonZeroExitCode_StopsOperation() {
        CreateHook("pre-flow-feature-start", "exit 7", "exit 7");

        var result = _repo.ExecuteGitFlow("feature start my-feature");

        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("pre-flow-feature-start");
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    [Fact]
    public void InitPreHook_NonZeroExitCode_StopsReinitialization() {
        CreateHook("pre-flow-init", "exit 7", "exit 7");

        var result = _repo.ExecuteGitFlow("init -d -f --feature custom/");

        result.Success.ShouldBeFalse();
        _repo.ExecuteGit("config --get gitflow.prefix.feature").Output.ShouldBe("feature/");
    }

    [Fact]
    public void InitFilterHook_ReplacesExplicitConfigurationValue() {
        _repo.ExecuteGit("branch production");
        CreateHook(
            "filter-flow-init-main",
            "if($env:GITFLOW_COMMAND -ne 'init') { exit 2 }; Write-Output 'production'",
            "test \"$GITFLOW_COMMAND\" = init || exit 2; printf '%s\\n' production");

        var result = _repo.ExecuteGitFlow("init -d -f --main original-main");

        result.Success.ShouldBeTrue(result.Output);
        _repo.ExecuteGit("config --get gitflow.branch.master").Output.ShouldBe("production");
    }

    [Fact]
    public void PushPreHook_NonZeroExitCode_StopsOperation() {
        CreateHook("pre-flow-push", "exit 7", "exit 7");

        var result = _repo.ExecuteGitFlow("push");

        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("pre-flow-push");
    }

    [Fact]
    public void FilterHook_StandardOutput_ReplacesBranchName() {
        CreateHook(
            "filter-flow-feature-start-name",
            "Write-Output 'renamed-feature'",
            "printf '%s\\n' 'renamed-feature'");

        var result = _repo.ExecuteGitFlow("feature start original-feature");

        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/renamed-feature").ShouldBeTrue();
        _repo.BranchExists("feature/original-feature").ShouldBeFalse();
    }

    [Fact]
    public void ShellHook_RunsRegardlessOfOperatingSystem() {
        CreateShellHook("pre-flow-feature-start", "test \"$GITFLOW_COMMAND\" = feature && test \"$GITFLOW_ACTION\" = start");

        var result = _repo.ExecuteGitFlow("feature start my-feature");

        result.Success.ShouldBeTrue(result.Output);
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
    }

    [Fact]
    public void PowerShellHook_UsesPowerShellRuntime() {
        if(!IsPowerShellAvailable()) {
            throw Xunit.Sdk.SkipException.ForSkip("PowerShell is not installed on this test runner.");
        }

        CreatePowerShellHook("pre-flow-feature-start", "if($env:GITFLOW_COMMAND -ne 'feature' -or $env:GITFLOW_ACTION -ne 'start') { exit 2 }");

        var result = _repo.ExecuteGitFlow("feature start my-feature");

        result.Success.ShouldBeTrue(result.Output);
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
    }

    [Fact]
    public void FilterHook_NonZeroExitCode_StopsOperation() {
        CreateHook(
            "filter-flow-feature-start-name",
            "Write-Error 'Rejected'; exit 8",
            "echo 'Rejected' >&2; exit 8");

        var result = _repo.ExecuteGitFlow("feature start my-feature");

        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("filter-flow-feature-start-name");
        _repo.BranchExists("feature/my-feature").ShouldBeFalse();
    }

    [Fact]
    public void FilterHook_PreservesArgumentsAndUsesConfiguredHookDirectory() {
        _repo.ExecuteGit("config gitflow.path.hooks \".git/custom hooks\"");
        CreateHook(
            "filter-flow-release-finish-message",
            "if($args[0] -ne 'original tag message') { exit 2 }; Write-Output \"filtered release`nmessage line two\"",
            "test \"$1\" = 'original tag message' || exit 2; printf 'filtered release\\nmessage line two\\n'",
            ".git/custom hooks");
        _repo.ExecuteGitFlow("release start 1.0.0");
        _repo.CreateCommit("Release work");

        var result = _repo.ExecuteGitFlow("release finish 1.0.0 -m \"original tag message\"");

        result.Success.ShouldBeTrue();
        var tagMessage = _repo.ExecuteGit("tag -n99 1.0.0").Output;
        tagMessage.ShouldContain("filtered release");
        tagMessage.ShouldContain("message line two");
    }

    [Fact]
    public void PostHook_RunsAfterOperationAndCanReportFailure() {
        CreateHook(
            "post-flow-feature-start",
            "Set-Content -NoNewline -Path (Join-Path $env:GITFLOW_REPOSITORY 'hook-ran') 'yes'; exit 9",
            "printf 'yes' > \"$GITFLOW_REPOSITORY/hook-ran\"; exit 9");

        var result = _repo.ExecuteGitFlow("feature start my-feature");

        result.Success.ShouldBeFalse();
        result.Output.ShouldContain("post-flow-feature-start");
        _repo.BranchExists("feature/my-feature").ShouldBeTrue();
        _repo.ExecuteGit("status --short").Output.ShouldContain("hook-ran");
    }

    [Fact]
    public void PreHook_ReceivesCommandAndFilteredParameters() {
        CreateHook(
            "filter-flow-feature-start-name",
            "Write-Output 'filtered-name'",
            "printf '%s\\n' 'filtered-name'");
        CreateHook(
            "pre-flow-feature-start",
            "if($env:GITFLOW_COMMAND -ne 'feature' -or $env:GITFLOW_ACTION -ne 'start' -or $env:GITFLOW_PARAM_NAME -ne 'filtered-name') { exit 4 }",
            "test \"$GITFLOW_COMMAND\" = feature && test \"$GITFLOW_ACTION\" = start && test \"$GITFLOW_PARAM_NAME\" = filtered-name");

        var result = _repo.ExecuteGitFlow("feature start original-name");

        result.Success.ShouldBeTrue();
        _repo.BranchExists("feature/filtered-name").ShouldBeTrue();
    }

    private void CreateHook(string hookName, string windowsScript, string posixScript, string hookDirectory = ".git/hooks") {
        var extension = OperatingSystem.IsWindows() ? ".ps1" : ".sh";
        var script = OperatingSystem.IsWindows() ? windowsScript : $"#!/bin/sh\n{posixScript}";
        _repo.CreateFile($"{hookDirectory}/{hookName}{extension}", script);
    }

    private void CreateShellHook(string hookName, string script) {
        _repo.CreateFile($".git/hooks/{hookName}.sh", $"#!/bin/sh\n{script}");
    }

    private void CreatePowerShellHook(string hookName, string script) {
        _repo.CreateFile($".git/hooks/{hookName}.ps1", script);
    }

    private static bool IsPowerShellAvailable() {
        var executables = OperatingSystem.IsWindows()
            ? new[] { "pwsh.exe", "powershell.exe" }
            : new[] { "pwsh" };

        foreach(var executable in executables) {
            try {
                using var process = Process.Start(new ProcessStartInfo(executable, "-NoProfile -NonInteractive -Command exit 0") {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                if(process is null) {
                    continue;
                }

                process.WaitForExit();
                if(process.ExitCode == 0) {
                    return true;
                }
            } catch(Win32Exception) {
                continue;
            }
        }

        return false;
    }
}
