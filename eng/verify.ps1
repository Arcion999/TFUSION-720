[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot 'TFUSION-720.sln'
$artifactRoot = Join-Path $repositoryRoot 'artifacts'

Push-Location $repositoryRoot
try {
    & (Join-Path $PSScriptRoot 'clean.ps1')

    dotnet format $solution --verify-no-changes --verbosity diagnostic
    if ($LASTEXITCODE -ne 0) { throw 'Formatting verification failed.' }

    & (Join-Path $PSScriptRoot 'build.ps1')
    & (Join-Path $PSScriptRoot 'test.ps1') -NoBuild

    New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
    $auditPath = Join-Path $artifactRoot 'dependency-audit.json'
    dotnet list $solution package --vulnerable --include-transitive --format json | Out-File `
        -LiteralPath $auditPath -Encoding utf8
    if ($LASTEXITCODE -ne 0) { throw 'Dependency vulnerability audit failed.' }

    $auditText = Get-Content -LiteralPath $auditPath -Raw
    if ($auditText -match '"severity"\s*:\s*"(?:high|critical|3|4)"') {
        throw 'Dependency audit reported a high or critical vulnerability.'
    }

    $determinismRoot = Join-Path $artifactRoot 'determinism'
    $firstOutput = Join-Path $determinismRoot 'first'
    $secondOutput = Join-Path $determinismRoot 'second'
    $foundationProject = Join-Path $repositoryRoot 'src/TFusion.Foundation/TFusion.Foundation.csproj'
    $pathMap = "$repositoryRoot=/_/"

    dotnet build $foundationProject --configuration Release --no-restore `
        --output $firstOutput --property:PathMap=$pathMap
    if ($LASTEXITCODE -ne 0) { throw 'First deterministic build failed.' }

    dotnet build $foundationProject --configuration Release --no-restore `
        --output $secondOutput --property:PathMap=$pathMap
    if ($LASTEXITCODE -ne 0) { throw 'Second deterministic build failed.' }

    $firstHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $firstOutput 'TFusion.Foundation.dll')).Hash
    $secondHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $secondOutput 'TFusion.Foundation.dll')).Hash
    if ($firstHash -ne $secondHash) {
        throw "Deterministic output check failed: $firstHash != $secondHash"
    }

    Write-Host "Deterministic Foundation DLL SHA-256: $firstHash"
}
finally {
    Pop-Location
}
