[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$toolsRoot = Join-Path $repositoryRoot '.tools'
$vcpkgRoot = Join-Path $toolsRoot 'vcpkg'
$pinnedCommit = '04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4'

New-Item -ItemType Directory -Force -Path $toolsRoot | Out-Null
if (-not (Test-Path -LiteralPath (Join-Path $vcpkgRoot '.git') -PathType Container)) {
    if (Test-Path -LiteralPath $vcpkgRoot) {
        throw "Refusing to replace non-vcpkg path: $vcpkgRoot"
    }
    git clone --filter=blob:none --no-checkout https://github.com/microsoft/vcpkg.git $vcpkgRoot
    if ($LASTEXITCODE -ne 0) { throw 'vcpkg clone failed.' }
}

git -C $vcpkgRoot fetch --depth 1 origin $pinnedCommit
if ($LASTEXITCODE -ne 0) { throw 'Pinned vcpkg baseline fetch failed.' }
git -C $vcpkgRoot checkout --detach $pinnedCommit
if ($LASTEXITCODE -ne 0) { throw 'Pinned vcpkg baseline checkout failed.' }

$actualCommit = (git -C $vcpkgRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualCommit -ne $pinnedCommit) {
    throw "vcpkg baseline mismatch. Expected $pinnedCommit; found $actualCommit."
}

$bootstrap = Join-Path $vcpkgRoot 'bootstrap-vcpkg.bat'
& $bootstrap -disableMetrics
if ($LASTEXITCODE -ne 0) { throw 'vcpkg bootstrap failed.' }

Write-Host "Pinned vcpkg ready at $vcpkgRoot ($actualCommit)."
