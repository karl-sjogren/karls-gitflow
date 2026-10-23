# Git flow commands as a global .NET tool [![codecov](https://codecov.io/github/karl-sjogren/karls-gitflow/graph/badge.svg?token=XnSJ1qvn15)](https://codecov.io/github/karl-sjogren/karls-gitflow) [![NuGet Version](https://img.shields.io/nuget/v/Karls.Gitflow.Tool)](https://www.nuget.org/packages/Karls.Gitflow.Tool#readme-body-tab)

A modern .NET command-line tool for managing Git branches using the Gitflow
branching model. This tool is a reimplementation of parts of
[gitflow-avh](https://github.com/petervanderdoes/gitflow-avh). It isn't 100%
compatible, but provides similar functionality with some additional features.

## Features

- **Feature branches** - Develop new features in isolation
- **Bugfix branches** - Fix bugs targeting the develop branch
- **Release branches** - Prepare releases with version bumps and final fixes
- **Hotfix branches** - Quick fixes for production issues
- **Support branches** - Long-term support for older versions

### Supported Operations

| Branch Type | list | start | finish | publish | track | delete |
|-------------|:----:|:-----:|:------:|:-------:|:-----:|:------:|
| Feature     |  Y   |   Y   |   Y    |    Y    |   Y   |   Y    |
| Bugfix      |  Y   |   Y   |   Y    |    Y    |   Y   |   Y    |
| Release     |  Y   |   Y   |   Y    |    Y    |   Y   |   Y    |
| Hotfix      |  Y   |   Y   |   Y    |    Y    |   Y   |   Y    |
| Support     |  Y   |   Y   |   -    |    Y    |   Y   |   Y    |

## Installation

### .NET Global Tool

```bash
dotnet tool install -g Karls.Gitflow.Tool
```

### Homebrew (OSX only)

```bash
brew install karl-sjogren/tap/karls-gitflow
```

### Windows

Get the installer from the [latest release](https://github.com/karl-sjogren/karls-gitflow/releases/latest) on Github.

### macOS installer

Download the `.pkg` for your Mac's architecture from the [latest release](https://github.com/karl-sjogren/karls-gitflow/releases/latest) and open it to install `git-flow` to `/usr/local/bin`.

## Usage

### Initialize Gitflow

```bash
# Interactive initialization
git flow init

# Use default settings
git flow init -d

# Specify options
git flow init --main main --develop develop
```

### Feature Branches

```bash
# List all feature branches
git flow feature list

# Start a new feature
git flow feature start my-feature

# Start a new feature, fetching from origin first
git flow feature start my-feature -F

# Finish a feature (merges to develop)
git flow feature finish my-feature

# Publish feature to remote
git flow feature publish my-feature

# Track a remote feature branch (created by a teammate)
git flow feature track my-feature

# Delete a feature branch
git flow feature delete my-feature
```

### Release Branches

```bash
# Start a release
git flow release start 1.0.0

# Start a release, fetching from origin first
git flow release start 1.0.0 -F

# Finish a release (merges to main AND develop, creates tag)
git flow release finish 1.0.0 -m "Release 1.0.0"

# Publish release to remote
git flow release publish 1.0.0

# Track a remote release branch
git flow release track 1.0.0
```

### Hotfix Branches

```bash
# Start a hotfix from main
git flow hotfix start 1.0.1

# Start a hotfix, fetching from origin first
git flow hotfix start 1.0.1 -F

# Finish a hotfix (merges to main AND develop, creates tag)
git flow hotfix finish 1.0.1 -m "Hotfix 1.0.1"

# Track a remote hotfix branch
git flow hotfix track 1.0.1
```

### Configuration

```bash
# List current configuration
git flow config list

# Set a configuration value
git flow config set feature feat/

# Force an immediate check for tool updates
git flow update-check
```

## Branch Auto-Detection

When on a gitflow branch, you can omit the branch name for finish, publish, and delete commands:

```bash
# While on feature/my-feature branch
git flow feature finish  # Automatically detects "my-feature"
```

## Start Options

| Option | Description |
|--------|-------------|
| `-F, --fetch` | Fetch from origin before creating the branch |

## Finish Options

| Option | Description |
|--------|-------------|
| `-k, --keep` | Keep both local and remote branch after finishing |
| `--keeplocal` | Keep local branch but delete remote after finishing |
| `--keepremote` | Keep remote branch but delete local after finishing |
| `-F, --fetch` | Fetch from origin before finishing |
| `-p, --push` | Push to origin after finishing |
| `-S, --squash` | Squash commits during merge |
| `-r, --rebase` | Rebase onto target branch before merging (feature/bugfix only) |

### Release/Hotfix Specific Options

| Option | Description |
|--------|-------------|
| `-m, --message` | Tag message |
| `-n, --notag` | Don't create a tag |
| `-b, --nobackmerge` | Don't merge back to develop |

## Project Structure

```
src/
  Karls.Gitflow.Core/        # Core library with gitflow logic
  Karls.Gitflow.Tool/        # CLI application

test/
  Karls.Gitflow.Core.Tests/  # Unit tests for core library
  Karls.Gitflow.Tool.Tests/  # E2E tests for CLI
  Karls.Gitflow.TestHelpers/ # Shared test utilities
```

## Building from Source

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the tool locally
dotnet run --project src/Karls.Gitflow.Tool -- init -d

# Create a macOS installer from published artifacts (run on macOS)
dotnet publish src/Karls.Gitflow.Tool/Karls.Gitflow.Tool.csproj \
  --configuration Release --runtime osx-arm64 --self-contained \
  --output ./artifacts/publish
./scripts/create-macos-pkg.sh ./artifacts/publish 0.0.14 \
  ./artifacts/karls-gitflow-0.0.14-osx-arm64.pkg
```

## Requirements

- .NET 11 SDK (RC 2)
- Git installed and available in PATH

## License

MIT
