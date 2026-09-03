[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$solution = Join-Path $repositoryRoot 'TFUSION-720.sln'
if (-not (Test-Path -LiteralPath $solution -PathType Leaf)) {
    throw 'Repository root validation failed: TFUSION-720.sln is missing.'
}

$targets = [Collections.Generic.List[string]]::new()
$artifactPath = Join-Path $repositoryRoot 'artifacts'
if (Test-Path -LiteralPath $artifactPath) { $targets.Add($artifactPath) }

foreach ($directory in Get-ChildItem -LiteralPath $repositoryRoot -Directory -Recurse -Force) {
    if ($directory.Name -in @('bin', 'obj', 'TestResults')) {
        $targets.Add($directory.FullName)
    }
}

$rootPrefix = $repositoryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
foreach ($target in $targets | Sort-Object { $_.Length } -Descending -Unique) {
    $resolvedTarget = [IO.Path]::GetFullPath($target)
    if (-not $resolvedTarget.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a path outside the repository: $resolvedTarget"
    }

    if ($resolvedTarget -eq $repositoryRoot -or $resolvedTarget.Length -le $rootPrefix.Length) {
        throw "Refusing broad cleanup target: $resolvedTarget"
    }

    Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
    Write-Host "Removed $resolvedTarget"
}
