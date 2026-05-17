param(
  [Parameter(Mandatory = $true)]
  [string]$PublishDirectory
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Invoke-External {
  param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,
    [string[]]$Arguments = @(),
    [string]$WorkingDirectory = (Get-Location).Path
  )

  Push-Location -Path $WorkingDirectory
  try {
    & $FilePath @Arguments
    if($LASTEXITCODE -ne 0) {
      $joinedArguments = if($Arguments.Count -gt 0) { " $($Arguments -join ' ')" } else { '' }
      throw "Command failed with exit code ${LASTEXITCODE}: $FilePath$joinedArguments"
    }
  } finally {
    Pop-Location
  }
}

$publishPath = Resolve-Path -Path $PublishDirectory
$executableName = if($IsWindows) { 'git-flow.exe' } else { 'git-flow' }
$executablePath = Join-Path -Path $publishPath -ChildPath $executableName

if(-not (Test-Path -Path $executablePath -PathType Leaf)) {
  throw "Published executable not found at '$executablePath'."
}

Write-Host "Verifying published executable at '$executablePath'."
Invoke-External -FilePath $executablePath -Arguments @('--help')

$tempRoot = if($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [System.IO.Path]::GetTempPath() }
$repoPath = Join-Path -Path $tempRoot -ChildPath "gitflow-smoke-$([Guid]::NewGuid().ToString('N'))"

New-Item -Path $repoPath -ItemType Directory -Force | Out-Null

try {
  Invoke-External -FilePath 'git' -Arguments @('init', '--initial-branch', 'main') -WorkingDirectory $repoPath
  Invoke-External -FilePath 'git' -Arguments @('config', 'user.name', 'Gitflow CI') -WorkingDirectory $repoPath
  Invoke-External -FilePath 'git' -Arguments @('config', 'user.email', 'gitflow-ci@example.com') -WorkingDirectory $repoPath

  Set-Content -Path (Join-Path -Path $repoPath -ChildPath 'README.md') -Value "# Smoke test`n" -NoNewline
  Invoke-External -FilePath 'git' -Arguments @('add', 'README.md') -WorkingDirectory $repoPath
  Invoke-External -FilePath 'git' -Arguments @('commit', '-m', 'initial commit') -WorkingDirectory $repoPath

  Invoke-External -FilePath $executablePath -Arguments @('init', '--defaults', '--save') -WorkingDirectory $repoPath

  $configPath = Join-Path -Path $repoPath -ChildPath '.gitflow'
  if(-not (Test-Path -Path $configPath -PathType Leaf)) {
    throw "Expected gitflow configuration file not found at '$configPath'."
  }

  Invoke-External -FilePath $executablePath -Arguments @('feature', 'start', 'aot-smoke-test') -WorkingDirectory $repoPath
  $currentBranch = (git -C $repoPath rev-parse --abbrev-ref HEAD).Trim()
  if($LASTEXITCODE -ne 0) {
    throw "Failed to get current branch in smoke-test repository."
  }

  if($currentBranch -ne 'feature/aot-smoke-test') {
    throw "Expected branch 'feature/aot-smoke-test' but found '$currentBranch'."
  }

  Write-Host 'Published git-flow smoke test passed.'
} finally {
  Remove-Item -Path $repoPath -Recurse -Force -ErrorAction SilentlyContinue
}
