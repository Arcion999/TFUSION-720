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
$buildOption = if ($NoBuild) { @('--no-build') } else { @() }

Push-Location $repositoryRoot
try {
    dotnet test $foundationTests --configuration Release @buildOption `
        --logger 'trx;LogFileName=foundation.trx' `
        --results-directory $testResults `
        --collect 'XPlat Code Coverage' `
        -- 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura' `
        'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=[TFusion.Foundation]*'
    if ($LASTEXITCODE -ne 0) { throw 'Foundation tests failed.' }

    dotnet test $architectureTests --configuration Release @buildOption `
        --logger 'trx;LogFileName=architecture.trx' `
        --results-directory $testResults
    if ($LASTEXITCODE -ne 0) { throw 'Architecture tests failed.' }

    $coverageFiles = @(Get-ChildItem -Path $testResults -Filter 'coverage.cobertura.xml' -Recurse)
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
