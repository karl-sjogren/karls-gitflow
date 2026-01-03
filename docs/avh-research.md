# Gitflow-AVH Analysis

This document analyzes the gitflow-avh project (https://github.com/petervanderdoes/gitflow-avh) to serve as a reference for reimplementation in .NET.

## Overview

Gitflow-AVH is a collection of Git extensions implementing Vincent Driessen's branching model. It provides high-level repository operations for managing feature development, releases, and hotfixes in a structured workflow.

The project is written entirely in shell scripts (~98% shell code) and was archived on June 19, 2023.

## Branching Model

```
                                    main (production)
                                      |
     hotfix/1.0.1 ──────────────────┬─┴─────────────────────────────────> tag: 1.0.1
                                    │
                release/1.0.0 ──────┼───┬───────────────────────────────> tag: 1.0.0
                                    │   │
     feature/login ─────────────────┼───┼───┐
                                    │   │   │
     develop ───────────────────────┴───┴───┴──────────────────────────>
```

### Branch Types

| Type | Base | Merges Into | Purpose |
|------|------|-------------|---------|
| `feature` | develop | develop | New features |
| `bugfix` | develop | develop | Bug fixes (non-urgent) |
| `release` | develop | main + develop | Prepare release |
| `hotfix` | main | main + develop | Urgent production fix |
| `support` | tag/commit | (long-lived) | Maintain old versions |

---

## Commands

### `git flow init`

Initializes a repository for gitflow usage.

**Usage:**
```bash
git flow init [-d] [-f]
git flow init [-d] [-f] -p <feature> -b <bugfix> -r <release> -x <hotfix> -s <support> -t <tag>
```

**Flags:**
| Flag | Description |
|------|-------------|
| `-d, --defaults` | Use default branch names and prefixes |
| `-f, --force` | Force reconfiguration even if already initialized |
| `-p, --feature` | Feature branch prefix |
| `-b, --bugfix` | Bugfix branch prefix |
| `-r, --release` | Release branch prefix |
| `-x, --hotfix` | Hotfix branch prefix |
| `-s, --support` | Support branch prefix |
| `-t, --tag` | Version tag prefix |
| `--local/--global/--system` | Configuration scope |

**Behavior:**
1. Checks if already initialized (exits unless `-f`)
2. Prompts for production branch (tries: production, main, master)
3. Prompts for development branch (tries: develop, int, integration)
4. Prompts for branch prefixes (defaults: `feature/`, `bugfix/`, `release/`, `hotfix/`, `support/`)
5. Prompts for version tag prefix (default: empty)
6. Creates develop branch from main if it doesn't exist
7. Writes configuration to git config

**Configuration Keys:**
```
gitflow.branch.master = main
gitflow.branch.develop = develop
gitflow.prefix.feature = feature/
gitflow.prefix.bugfix = bugfix/
gitflow.prefix.release = release/
gitflow.prefix.hotfix = hotfix/
gitflow.prefix.support = support/
gitflow.prefix.versiontag =
gitflow.path.hooks = (path to hooks directory)
```

---

### `git flow feature`

Manages feature branches for new development.

**Subcommands:**

#### `feature list`
```bash
git flow feature [list] [-v]
```
Lists all feature branches. Verbose mode shows more details.

#### `feature start`
```bash
git flow feature start [-F] <name> [<base>]
```
Creates a new feature branch from base (default: develop).

| Flag | Description |
|------|-------------|
| `-F, --fetch` | Fetch from origin before creating |

**Workflow:**
1. Validate base branch exists
2. Optionally fetch from origin
3. Create branch `<prefix><name>` from `<base>`
4. Checkout new branch

#### `feature finish`
```bash
git flow feature finish [-rFkDS] [--no-ff] [--push] <name|nameprefix>
```
Merges feature into develop and cleans up.

| Flag | Description |
|------|-------------|
| `-r, --rebase` | Rebase before merging |
| `-p, --preserve-merges` | Preserve merges during rebase |
| `-F, --fetch` | Fetch from origin first |
| `-k, --keep` | Keep branch after merge |
| `--keepremote` | Keep remote branch |
| `--keeplocal` | Keep local branch |
| `-D, --force_delete` | Force delete branch |
| `-S, --squash` | Squash commits during merge |
| `--no-ff` | Force no-fast-forward merge |
| `--push` | Push to origin after finish |

**Workflow:**
1. Validate working tree is clean
2. Optionally fetch from origin
3. Optionally rebase on develop
4. Checkout develop
5. Merge feature (no-ff unless single commit and no --no-ff flag)
6. Delete feature branch (local and/or remote)
7. Optionally push develop

#### `feature publish`
```bash
git flow feature publish <name>
```
Pushes feature branch to origin with upstream tracking.

**Workflow:**
1. Create remote tracking branch
2. Push to origin
3. Set upstream

#### `feature delete`
```bash
git flow feature delete [-f] [-r] <name>
```
Deletes a feature branch.

| Flag | Description |
|------|-------------|
| `-f, --force` | Force delete |
| `-r, --remote` | Delete remote branch too |

---

### `git flow bugfix`

Manages bugfix branches. Nearly identical to feature branches.

**Subcommands:** Same as feature (list, start, finish, publish, delete)

**Key Difference:** Uses `gitflow.prefix.bugfix` prefix, defaults to `bugfix/`.

---

### `git flow release`

Manages release branches for preparing production releases.

**Subcommands:**

#### `release list`
```bash
git flow release [list] [-v]
```

#### `release start`
```bash
git flow release start [-F] <version> [<base>]
```
Creates release branch from develop (default).

#### `release finish`
```bash
git flow release finish [-Fsumpkn] [--push] [--nobackmerge] <version>
```
Completes a release: merges to main AND develop, creates tag.

| Flag | Description |
|------|-------------|
| `-F, --fetch` | Fetch from origin first |
| `-s, --sign` | Sign the tag with GPG |
| `-u, --signingkey` | GPG key to use for signing |
| `-m, --message` | Tag message |
| `-f, --messagefile` | Read tag message from file |
| `-p, --push` | Push branches and tags to origin |
| `-k, --keep` | Keep branch after merge |
| `-n, --notag` | Don't create a tag |
| `-b, --nobackmerge` | Don't merge back to develop |
| `-S, --squash` | Squash commits |
| `--ff-master` | Allow fast-forward on master |

**Workflow:**
1. Validate working tree is clean
2. Optionally fetch from origin
3. Checkout main/master
4. Merge release branch (no-ff)
5. Create annotated tag on main
6. Checkout develop
7. Merge tag (or main) into develop
8. Delete release branch
9. Optionally push main, develop, and tags

#### `release publish`
```bash
git flow release publish <version>
```

#### `release delete`
```bash
git flow release delete [-f] [-r] <version>
```

---

### `git flow hotfix`

Manages hotfix branches for urgent production fixes.

**Subcommands:**

#### `hotfix list`
```bash
git flow hotfix [list] [-v]
```

#### `hotfix start`
```bash
git flow hotfix start [-F] <version> [<base>]
```
Creates hotfix branch from main (default).

**Note:** Only one hotfix at a time unless `gitflow.multi-hotfix` is enabled.

#### `hotfix finish`
```bash
git flow hotfix finish [-Fsumpkn] [--push] [--nobackmerge] <version>
```
Completes a hotfix: merges to main AND develop, creates tag.

**Workflow:** Same as release finish.

#### `hotfix publish`
```bash
git flow hotfix publish <version>
```

#### `hotfix delete`
```bash
git flow hotfix delete [-f] [-r] <version>
```

---

### `git flow support`

Manages support branches for maintaining old versions.

**Subcommands:**

#### `support list`
```bash
git flow support [list] [-v]
```

#### `support start`
```bash
git flow support start <version> <base>
```
Creates support branch from specific tag/commit. Base is required.

**Note:** Support branches are long-lived. There is no `finish` command.

---

### `git flow config`

Manages gitflow configuration.

**Subcommands:**

#### `config list`
```bash
git flow config [list]
```
Shows all gitflow configuration.

#### `config set`
```bash
git flow config set <option> <value>
```
Sets a configuration option.

**Options:**
- `master` - Production branch name
- `develop` - Development branch name
- `feature` - Feature prefix
- `bugfix` - Bugfix prefix
- `release` - Release prefix
- `hotfix` - Hotfix prefix
- `support` - Support prefix
- `versiontagprefix` - Version tag prefix
- `allowmultihotfix` - Allow multiple hotfixes (true/false)

#### `config base`
```bash
git flow config base <branch> [<base>]
```
Get or set the base branch for a specific branch.

---

## Validation & Error Handling

### Pre-conditions Checked

1. **Git Repository** - Must be inside a git working tree
2. **Gitflow Initialized** - Configuration must exist
3. **Clean Working Tree** - No uncommitted changes (for most operations)
4. **Branch Exists** - Target branch must exist
5. **Branch Absent** - New branch must not already exist
6. **Branches Equal** - Local and remote branches must be in sync

### Working Tree States

```c
// git_is_clean_working_tree() return codes:
0 = Clean (no changes)
1 = Unstaged changes exist
2 = Staged but uncommitted changes exist
```

### Branch Comparison States

```c
// git_compare_refs() return codes:
0 = Branches are identical
1 = First branch needs fast-forward
2 = Branches have diverged
3 = No common ancestor (unrelated)
4 = Second branch needs fast-forward
```

---

## Merge Strategies

### Feature/Bugfix Finish
- Default: `--no-ff` (no fast-forward) to preserve branch history
- Single commit exception: fast-forward allowed unless `--no-ff` explicitly set
- Squash option: combine all commits into one

### Release/Hotfix Finish
- Always `--no-ff` to main
- Tag created on main after merge
- Back-merge uses tag reference (not branch) to ensure tagged commit is in develop

---

## Hook System

Gitflow supports hooks at various points (not implementing initially):

| Hook | When |
|------|------|
| `pre-flow-<command>-<action>` | Before action (can abort) |
| `post-flow-<command>-<action>` | After action (cleanup) |
| `filter-flow-<command>-<action>-<param>` | Transform parameters |

Example hooks:
- `pre-flow-feature-start`
- `post-flow-release-finish`
- `filter-flow-release-finish-version`

---

## Implementation Notes for .NET

### Command Structure
```
git-flow <command> <action> [flags] [arguments]
         │         │
         │         └── list, start, finish, publish, delete
         └── init, feature, bugfix, release, hotfix, support, config
```

### Shared Logic (minimize duplication)

1. **Repository Validation**
   - `IsGitRepository()`
   - `IsGitflowInitialized()`
   - `IsWorkingTreeClean()`

2. **Branch Operations**
   - `ListBranches(prefix)`
   - `StartBranch(name, base, prefix)`
   - `DeleteBranch(name, force, remote)`
   - `PublishBranch(name)`

3. **Finish Workflows**
   - Simple: feature/bugfix (merge to develop)
   - Dual: release/hotfix (merge to main + develop + tag)

### Configuration Default Values
```csharp
public static class GitFlowDefaults
{
    public const string MainBranch = "main";
    public const string DevelopBranch = "develop";
    public const string FeaturePrefix = "feature/";
    public const string BugfixPrefix = "bugfix/";
    public const string ReleasePrefix = "release/";
    public const string HotfixPrefix = "hotfix/";
    public const string SupportPrefix = "support/";
    public const string VersionTagPrefix = "";
}
```

### Git Commands Used

| Operation | Git Command |
|-----------|-------------|
| Current branch | `git rev-parse --abbrev-ref HEAD` |
| List local branches | `git for-each-ref --format='%(refname:short)' refs/heads` |
| List remote branches | `git for-each-ref --format='%(refname:short)' refs/remotes` |
| Check clean | `git status --porcelain` |
| Create branch | `git checkout -b <name> <base>` |
| Checkout | `git checkout <branch>` |
| Merge no-ff | `git merge --no-ff <branch>` |
| Merge squash | `git merge --squash <branch>` |
| Create tag | `git tag -a <name> -m "<message>"` |
| Delete local | `git branch -d <name>` / `git branch -D <name>` |
| Delete remote | `git push origin --delete <name>` |
| Push | `git push -u origin <branch>` |
| Push tags | `git push origin --tags` |
| Fetch | `git fetch origin` |
| Get config | `git config --get <key>` |
| Set config | `git config <key> <value>` |
| Branch merged | `git branch --merged <target>` |
