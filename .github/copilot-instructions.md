# Copilot Instructions

## Project Overview

Karl's Gitflow is a .NET global command-line tool implementing the Gitflow branching model. It provides the `git-flow` command for feature, bugfix, release, hotfix, and support branches. Gitflow settings are stored in Git configuration; `.gitflow` JSON files support saving and loading configuration.

## Technology Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10 (`net10.0`); SDK 10.0.400 with `latestFeature` roll-forward |
| CLI | Spectre.Console.Cli and Spectre.Console |
| Tests | xUnit v3 on Microsoft.Testing.Platform, FakeItEasy, Shouldly |
| Analysis | Roslynator, Karls.Analyzers, BannedApiAnalyzers |

## Build and Test

Git must be installed and available on `PATH`.

```bash
dotnet restore
dotnet build
dotnet build --configuration Release
dotnet pack --output ./artifacts --configuration Release \
  ./src/Karls.Gitflow.Tool/Karls.Gitflow.Tool.csproj

# Core unit tests and CLI integration/end-to-end tests
dotnet run --project test/Karls.Gitflow.Core.Tests
dotnet run --project test/Karls.Gitflow.Tool.Tests

# Run one test (Microsoft.Testing.Platform / xUnit v3)
dotnet run --project test/Karls.Gitflow.Core.Tests -- \
  --filter-method Karls.Gitflow.Core.Tests.Services.FeatureBranchServiceTests.Prefix_ReturnsFeaturePrefix

# CI test run with coverage and TRX results
dotnet test --coverage --coverage-output-format cobertura \
  --coverage-settings ./test/coverage.config \
  --report-trx --configuration Release --no-progress
```

There is no separate lint command: `dotnet build` runs the configured Roslynator, Karls, and banned-API analyzers, with code-style enforcement and warnings as errors. NuGet package versions are centralized in `Directory.Packages.props`; keep package references in project files versionless.

## Architecture

- `Karls.Gitflow.Core` contains configuration, `GitService`, and branch lifecycle logic. `GitService` implements `IGitService` by issuing argument-list Git commands through `IGitExecutor`; `Karls.Gitflow.Tool/Infrastructure/GitExecutor` is the process-backed executor.
- `BranchServiceBase` centralizes validation and shared start/list/publish/track/delete behavior. Concrete branch services provide their configured prefix/base branch and finish behavior: feature/bugfix use a simple merge back to develop; release/hotfix merge to main and develop and create a version tag; support branches have no finish command.
- CLI leaf commands are thin Spectre.Console.Cli adapters over shared command bases in `Commands/GitFlowCommand.cs`. `Program.cs` registers the production command tree. Tool tests construct a parallel command tree in `GitRepositoryFixture`, so keep both registrations aligned when adding a command.
- `GitFlowContext` uses `AsyncLocal` overrides for the working directory and console; this lets CLI tests run against isolated temporary repositories without changing process-wide state.

## Codebase Conventions

- Follow `.editorconfig`: file-scoped namespaces, `var` when the type is apparent, no space after control-flow keywords (`if(condition)`), end-of-line braces, and private fields prefixed with `_`. C# files use CRLF and a 200-character line limit.
- Keep branch-specific behavior in the branch service classes and reusable lifecycle behavior in `BranchServiceBase`; command settings/output/error handling belong in the shared CLI command bases.
- Core service tests use FakeItEasy against `IGitService`. Tool command tests use `GitRepositoryFixture` and real temporary Git repositories; use that fixture for behavior requiring actual Git semantics.
- Use `TimeProvider` injection for time-dependent service behavior, rather than depending directly on the system clock.
- Add NuGet versions only to `Directory.Packages.props`; project files reference packages without versions.
