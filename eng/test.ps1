[CmdletBinding()]
param(
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testResults = Join-Path $repositoryRoot 'artifacts/test-results'
$foundationTests = Join-Path $repositoryRoot 'tests/TFusion.Foundation.Tests/TFusion.Foundation.Tests.csproj'
$architectureTests = Join-Path $repositoryRoot 'tests/TFusion.Architecture.Tests/TFusion.Architecture.Tests.csproj'

New-Item -ItemType Directory -Force -Path $testResults | Out-Null
$buildArgument = if ($NoBuild) { '--no-build' } else { $null }

Push-Location $repositoryRoot
try {
    $foundationArgs = @(
        'test',
        '--project', $foundationTests,
        '--configuration', 'Release'
    )
    if ($buildArgument) {
        $foundationArgs += $buildArgument
    }
    $foundationArgs += @(
        '--results-directory', $testResults,
        '--report-xunit-trx',
        '--report-xunit-trx-filename', 'foundation.trx',
        '--coverage',
        '--coverage-output-format', 'cobertura',
        '--coverage-output', 'foundation.coverage.cobertura.xml'
    )

    & dotnet @foundationArgs
    if ($LASTEXITCODE -ne 0) { throw 'Foundation tests failed.' }

    $architectureArgs = @(
        'test',
        '--project', $architectureTests,
        '--configuration', 'Release'
    )
    if ($buildArgument) {
        $architectureArgs += $buildArgument
    }
    $architectureArgs += @(
        '--results-directory', $testResults,
        '--report-xunit-trx',
        '--report-xunit-trx-filename', 'architecture.trx'
    )

    & dotnet @architectureArgs
    if ($LASTEXITCODE -ne 0) { throw 'Architecture tests failed.' }

    $coverageFiles = @(Get-ChildItem -Path $testResults -Filter 'foundation.coverage.cobertura.xml' -Recurse)
    if ($coverageFiles.Count -ne 1) {
        throw "Expected exactly one Foundation coverage report; found $($coverageFiles.Count)."
    }

    [xml]$coverage = Get-Content -LiteralPath $coverageFiles[0].FullName
    $lineRate = [double]::Parse($coverage.coverage.'line-rate', [Globalization.CultureInfo]::InvariantCulture)
    $branchRate = [double]::Parse($coverage.coverage.'branch-rate', [Globalization.CultureInfo]::InvariantCulture)
    if ($lineRate -lt 0.90 -or $branchRate -lt 0.85) {
        throw ('Foundation coverage below threshold. Line={0:P2}; Branch={1:P2}.' -f $lineRate, $branchRate)
    }

    Write-Host ('Foundation coverage passed. Line={0:P2}; Branch={1:P2}.' -f $lineRate, $branchRate)
}
finally {
    Pop-Location
}
