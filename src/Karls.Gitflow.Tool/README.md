# Git flow commands as a global .NET tool

## Installation

To install the Karls.Gitflow tool globally, run the following command:

```bash
dotnet tool install -g Karls.Gitflow.Tool
```

> Note: If other git flow tools are installed, they may take precedence
> in your PATH.

## Usage

### Initialize Gitflow

```bash
# Interactive initialization
git flow init
```

```bash
# Use default settings
git flow init -d
```

```bash
# Specify options
git flow init --main main --develop develop
```

### Feature Branches

Feature branches are used to develop new features and are branched off from
the develop branch. They are merged back into develop when finished.

```bash
# List all feature branches
git flow feature list
```

```bash
# Start a new feature
git flow feature start my-feature
```

```bash
# Finish a feature (merges to develop)
git flow feature finish my-feature
```

```bash
# Publish feature to remote
git flow feature publish my-feature
```

```bash
# Delete a feature branch
git flow feature delete my-feature
```

### Release Branches

Release branches are used to prepare for a new production release. They are
branched off from develop and merged into both main and develop when finished.
A tag is also created on main.

```bash
# Start a release
git flow release start 1.0.0
```

```bash
# Finish a release (merges to main AND develop, creates tag)
git flow release finish 1.0.0 -m "Release 1.0.0"
```

```bash
# Publish release to remote
git flow release publish 1.0.0
```

### Hotfix Branches

Hotfix branches are used to quickly patch production releases. They are
branched off from main and merged into both main and develop when finished.
A tag is also created on main.

```bash
# Start a hotfix from main
git flow hotfix start 1.0.1
```

```bash
# Finish a hotfix (merges to main AND develop, creates tag)
git flow hotfix finish 1.0.1 -m "Hotfix 1.0.1"
```
