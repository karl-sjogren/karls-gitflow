# Copilot Instructions for Karl's Gitflow

## Project Overview

**Karl's Gitflow** is a .NET 10 command-line tool that implements the [Gitflow branching model](https://nvie.com/posts/a-successful-git-branching-model/). It is a C# reimplementation of [gitflow-avh](https://github.com/petervanderdoes/gitflow-avh), distributed as a .NET global tool that provides the `git-flow` command.

### Key Capabilities
- Manage **feature**, **bugfix**, **release**, **hotfix**, and **support** branches
- Initialize gitflow configuration in a repository (`.gitflow` JSON config)
- Publish, track, and delete remote branches
- Check for tool updates via NuGet API

---

## Technology Stack

| Area | Technology |
|------|-----------|
| Language | C# (latest LangVersion) |
| Runtime | .NET 10.0 |
| SDK Version | 10.0.201 (pinned in `global.json`) |
| CLI Framework | Spectre.Console.Cli |
| UI / Output | Spectre.Console |
| Testing | xUnit v3 (via `xunit.v3.mtp-v2`) |
| Mocking | FakeItEasy |
| Assertions | Shouldly |
| File System Abstraction | TestableIO.System.IO.Abstractions |
| Static Analysis | Roslynator, Karls.Analyzers, BannedApiAnalyzers |
| Test Runner | Microsoft.Testing.Platform (configured in `global.json`) |

---

## Repository Structure

```
karls-gitflow/
├── .config/                            # Build/tool configuration
├── .editorconfig                       # Code style rules
├── .github/
│   ├── workflows/                      # CI/CD pipelines
│   │   ├── build.yml                   # Main CI: test, build, publish
│   │   ├── copilot-setup-steps.yml     # Copilot workspace setup
│   │   ├── create-release.yml          # Tag-triggered release
│   │   └── ...
│   ├── dependabot.yml
│   └── release.yml
├── .gitflow                            # Default gitflow JSON config
├── benchmarks/
│   └── Karls.Gitflow.Benchmarks/       # BenchmarkDotNet project
├── docs/
│   └── avh-research.md                 # Reference analysis of gitflow-avh
├── src/
│   ├── Karls.Gitflow.Core/             # Core library (services, interfaces)
│   │   └── Services/                   # Branch service implementations
│   ├── Karls.Gitflow.Tool/             # CLI executable (entry point)
│   │   ├── Commands/                   # Command classes per branch type
│   │   │   ├── Feature/
│   │   │   ├── Bugfix/
│   │   │   ├── Release/
│   │   │   ├── Hotfix/
│   │   │   └── Support/
│   │   └── Infrastructure/             # GitExecutor, UpdateChecker, etc.
│   └── Karls.Gitflow.WindowsInstaller/ # WiX MSI installer
├── test/
│   ├── Karls.Gitflow.Core.Tests/       # Unit tests for Core library
│   ├── Karls.Gitflow.Tool.Tests/       # Integration/E2E tests for CLI
│   └── Karls.Gitflow.TestHelpers/      # Shared test utilities
├── Directory.Build.props               # Root MSBuild props (version, build settings)
├── Directory.Packages.props            # Centralized NuGet package versions
├── Karls.Gitflow.slnx                  # Solution file
├── global.json                         # SDK version + test runner config
├── nuget.config                        # NuGet package sources
└── local-install.sh                    # Local dev installation script
```

---

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build all projects (debug)
dotnet build

# Build in release mode
dotnet build --configuration Release

# Pack the NuGet global tool package
dotnet pack --output ./artifacts --configuration Release ./src/Karls.Gitflow.Tool/Karls.Gitflow.Tool.csproj
```

Build settings (from `Directory.Build.props`):
- **Warnings as errors** is enabled — all compiler warnings must be resolved.
- `NU1901`–`NU1904` (NuGet vulnerability warnings) are excluded from warnings-as-errors.
- `CS1591`/`CS1573` (missing XML docs) are suppressed globally.
- Code style is enforced in build (`EnforceCodeStyleInBuild=true`).

---

## Running Tests

> ⚠️ **Important**: This project uses `Microsoft.Testing.Platform` as the test runner (set in `global.json`). Running `dotnet test` alone may fail due to test runner version conflicts. Use `dotnet run` to execute tests:

```bash
# Run Core unit tests
dotnet run --project test/Karls.Gitflow.Core.Tests

# Run Tool integration/E2E tests
dotnet run --project test/Karls.Gitflow.Tool.Tests

# Run all tests with coverage (CI-style)
dotnet test --coverage --coverage-output-format cobertura \
  --coverage-settings ./test/coverage.config \
  --report-trx --configuration Release --no-progress
```

Test projects:
- `Karls.Gitflow.Core.Tests` — Unit tests for services (`GitService`, `BranchServiceBase`, `GitFlowInitializer`, etc.)
- `Karls.Gitflow.Tool.Tests` — Integration/end-to-end tests for CLI commands
- `Karls.Gitflow.TestHelpers` — Shared utilities (cancellation tokens, embedded test resources)

---

## Local Tool Installation

To install the tool from local source (for manual testing):

```bash
./local-install.sh
# This uninstalls the global version, packs locally, and reinstalls from ./local-artifacts
```

Or manually:

```bash
dotnet tool uninstall --global Karls.Gitflow.Tool
dotnet pack -p:ToolCommandName=git-flow2 --output ./local-artifacts --configuration Release ./src/Karls.Gitflow.Tool/Karls.Gitflow.Tool.csproj
dotnet tool install --global --add-source ./local-artifacts Karls.Gitflow.Tool
```

---

## Code Style Conventions

### General Rules
- **File-scoped namespaces** are required (`namespace Karls.Gitflow.Core;`) — enforced as an error.
- **`var`** is preferred when the type is apparent.
- **`sealed`** by default for concrete classes not designed for inheritance.
- **No space** after keywords in control flow (`if(condition)`, `foreach(var x in y)`).
- **Braces**: End-of-line style, no new line before open brace.
- **Line endings**: CRLF (Windows-style) for C# and JSON files.
- **Indentation**: 4 spaces for C#, 2 spaces for JSON/YAML.
- **Max line length**: 200 characters for C#.

### Naming Conventions
| Symbol | Convention | Example |
|--------|-----------|---------|
| Class / Method / Property | PascalCase | `BranchServiceBase`, `StartBranch` |
| Parameter / Local variable | camelCase | `branchName`, `options` |
| Private instance field | `_camelCase` | `_gitService` |
| Private static readonly | `_camelCase` | `_defaultConfig` |
| Private constant | `_camelCase` | `_maxRetries` |
| Namespace | File-scoped PascalCase | `Karls.Gitflow.Core` |

### Analyzer Rules
- **Roslynator** (RCS0xxx/RCS1xxx) — code style and consistency
- **BannedApiAnalyzers** (RS0xxx) — restricted APIs listed in `src/BannedSymbols.txt`
- **Karls.Analyzers** (KP0001) — custom project-specific rules

---

## Key Patterns and Architecture

### Layered Architecture
1. **Presentation Layer** — Spectre.Console.Cli command classes in `src/Karls.Gitflow.Tool/Commands/`
2. **Business Logic** — `BranchServiceBase` subclasses and `GitFlowInitializer` in `src/Karls.Gitflow.Core/`
3. **Data Access** — `GitService` (runs git commands) in `src/Karls.Gitflow.Core/`
4. **Infrastructure** — `GitExecutor` (wraps `System.Diagnostics.Process`) in `src/Karls.Gitflow.Tool/Infrastructure/`

### Design Patterns
- **Template Method**: `BranchServiceBase` defines the overall branch lifecycle; subclasses override specific steps.
- **Strategy**: Each branch type (`FeatureBranchService`, `ReleaseBranchService`, etc.) implements `IBranchService`.
- **Command**: Spectre.Console.Cli commands wrap service calls with error handling and user feedback.
- **Abstraction**: `IGitService`, `IBranchService`, `IGitExecutor` interfaces enable testing with mocks.

### Command Base Class Hierarchy
```
GitFlowCommand<TSettings>
  ├── BranchStartCommand
  ├── BranchDeleteCommand
  ├── BranchListCommand
  ├── BranchPublishCommand
  ├── BranchTrackCommand
  └── BranchFinishCommand
        ├── BranchTagFinishCommand   (Release, Hotfix)
        └── BranchSimpleFinishCommand (Feature, Bugfix)
```

### Adding a New Command
1. Create a sealed class in the appropriate `Commands/<BranchType>/` folder.
2. Inherit from the relevant base command (e.g., `BranchStartCommand`).
3. Register the command in `Program.cs` under the correct branch type node.
4. Add corresponding unit/integration tests.

### Adding a New Service
1. Define or extend an interface in `src/Karls.Gitflow.Core/`.
2. Implement the service, inheriting from `BranchServiceBase` if it is a branch-type service.
3. Register the service in the DI setup in `Program.cs`.

---

## Configuration Files

| File | Purpose |
|------|---------|
| `global.json` | Pins .NET SDK to 10.0.201; sets `Microsoft.Testing.Platform` as test runner |
| `Directory.Build.props` | Solution-wide MSBuild settings (version `0.0.12`, strict build) |
| `Directory.Packages.props` | Central NuGet package version management (all versions here, not in .csproj) |
| `.editorconfig` | Code style and analyzer severity settings |
| `nuget.config` | NuGet.org as the sole package source |
| `.gitflow` | Default gitflow JSON config (branch names and prefixes) |
| `src/BannedSymbols.txt` | APIs forbidden by BannedApiAnalyzers |
| `test/coverage.config` | Code coverage exclusions |
| `test/testconfig.json` | Test culture setting (en-US) |

---

## Dependency Management

- All NuGet package **versions are centralized** in `Directory.Packages.props`. Individual `.csproj` files must **not** specify versions.
- Lock files are **disabled** (incompatible with Dependabot; see `Directory.Build.props` comment).
- Only `net10.0` is targeted; no multi-targeting.

To add a new package:
1. Add the version to `Directory.Packages.props`: `<PackageVersion Include="Some.Package" Version="x.y.z" />`
2. Reference it in the `.csproj` without a version: `<PackageReference Include="Some.Package" />`

---

## CI/CD Pipelines

The main workflow is `.github/workflows/build.yml`. It runs on pushes to `develop`, `main`, `release/**`, `feature/**`, `hotfix/**`, and `copilot/**` branches.

| Job | Platform | Description |
|-----|----------|-------------|
| `net-test` | Ubuntu + Windows | Run tests with coverage |
| `test-and-coverage-results` | Ubuntu | Aggregate coverage, upload to Codecov |
| `net-build` | Ubuntu + Windows | Pack NuGet / publish Windows exe |
| `windows-installer-build` | Windows (main only) | Build WiX `.msi` installer |
| `publish` | Ubuntu (main only) | Publish to NuGet.org via OIDC |

**Environment variables set in CI**:
```yaml
DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
DOTNET_CLI_TELEMETRY_OPTOUT: true
DOTNET_NOLOGO: true
```

---

## Known Issues and Workarounds

1. **`dotnet test` may fail** — The project uses `Microsoft.Testing.Platform` as the test runner, which can conflict with the traditional xUnit console runner when using `dotnet test`. Use `dotnet run --project test/<ProjectName>` instead to run tests reliably.

2. **Lock files disabled** — `RestorePackagesWithLockFile=false` because Dependabot does not support NuGet lock files (tracked in [dependabot-core#10863](https://github.com/dependabot/dependabot-core/issues/10863)).

3. **NuGet vulnerability warnings suppressed** — `NU1901`–`NU1904` are listed in `WarningsNotAsErrors` to prevent known transitive vulnerability warnings from breaking the build.

4. **Windows installer only on `main`** — The WiX installer build (`Karls.Gitflow.WindowsInstaller`) only runs on the `main` branch in CI to avoid unnecessary builds.
