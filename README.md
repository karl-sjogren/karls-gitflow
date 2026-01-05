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

| Branch Type | list | start | finish | publish | delete |
|-------------|:----:|:-----:|:------:|:-------:|:------:|
| Feature     |  Y   |   Y   |   Y    |    Y    |   Y    |
| Bugfix      |  Y   |   Y   |   Y    |    Y    |   Y    |
| Release     |  Y   |   Y   |   Y    |    Y    |   Y    |
| Hotfix      |  Y   |   Y   |   Y    |    Y    |   Y    |
| Support     |  Y   |   Y   |   -    |    -    |   Y    |

## Installation

```bash
dotnet tool install -g Karls.Gitflow.Tool
```

## Usage

### Initialize Gitflow

```bash
# Interactive initialization
git-flow init

# Use default settings
git-flow init -d

# Specify options
git-flow init --main main --develop develop
```

### Feature Branches

```bash
# List all feature branches
git-flow feature list

# Start a new feature
git-flow feature start my-feature

# Finish a feature (merges to develop)
git-flow feature finish my-feature

# Publish feature to remote
git-flow feature publish my-feature

# Delete a feature branch
git-flow feature delete my-feature
```

### Release Branches

```bash
# Start a release
git-flow release start 1.0.0

# Finish a release (merges to main AND develop, creates tag)
git-flow release finish 1.0.0 -m "Release 1.0.0"

# Publish release to remote
git-flow release publish 1.0.0
```

### Hotfix Branches

```bash
# Start a hotfix from main
git-flow hotfix start 1.0.1

# Finish a hotfix (merges to main AND develop, creates tag)
git-flow hotfix finish 1.0.1 -m "Hotfix 1.0.1"
```

### Configuration

```bash
# List current configuration
git-flow config list

# Set a configuration value
git-flow config set feature feat/
```

## Branch Auto-Detection

When on a gitflow branch, you can omit the branch name for finish, publish, and delete commands:

```bash
# While on feature/my-feature branch
git-flow feature finish  # Automatically detects "my-feature"
```

## Finish Options

| Option | Description |
|--------|-------------|
| `-k, --keep` | Keep the branch after finishing |
| `-F, --fetch` | Fetch from origin before finishing |
| `-p, --push` | Push to origin after finishing |
| `-S, --squash` | Squash commits during merge |

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
```

## Requirements

- .NET 10 SDK
- Git installed and available in PATH

## License

MIT
