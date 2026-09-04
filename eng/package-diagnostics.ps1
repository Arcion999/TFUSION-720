[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsWindows) { throw 'The diagnostics package is Windows-only.' }

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot 'src/TFusion.Diagnostics/TFusion.Diagnostics.csproj'
$packageRoot = Join-Path $repositoryRoot 'artifacts/diagnostics-package'

New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
dotnet publish $project --configuration Release --no-restore --property:Platform=x64 --output $packageRoot
if ($LASTEXITCODE -ne 0) { throw 'Diagnostics package creation failed.' }

foreach ($requiredFile in @('TFusion.Diagnostics.exe', 'TFusion.Kernel.Native.dll', 'TKernel.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $requiredFile) -PathType Leaf)) {
        throw "Diagnostics package is incomplete: $requiredFile is missing."
    }
}

$workingDirectory = Join-Path $env:TEMP ("tfusion-load-test-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $workingDirectory | Out-Null
$originalPath = $env:PATH
try {
    $env:PATH = "$env:SystemRoot\System32;$env:SystemRoot"
    Push-Location $workingDirectory
    try {
        $json = & (Join-Path $packageRoot 'TFusion.Diagnostics.exe') --self-test --format json
        if ($LASTEXITCODE -ne 0) { throw "Packaged diagnostics failed: $json" }
    }
    finally {
        Pop-Location
    }

    $report = $json | ConvertFrom-Json
    if ($report.status -ne 'pass'
        -or $report.nativeKernel.loadStatus -ne 'loaded'
        -or $report.nativeKernel.abiVersion -ne 1
        -or $report.nativeKernel.compiledOcctVersion -notmatch '8\.0\.1'
        -or $report.nativeKernel.runtimeOcctVersion -notmatch '8\.0\.1'
        -or $report.nativeKernel.initializationResult -ne 'success') {
        throw "Packaged diagnostics did not truthfully reach OCCT: $json"
    }

    $json | Out-File -LiteralPath (Join-Path $repositoryRoot 'artifacts/diagnostics-package-test.json') -Encoding utf8NoBOM

    $missingBridgePackage = Join-Path $workingDirectory 'missing-bridge-package'
    New-Item -ItemType Directory -Path $missingBridgePackage | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $packageRoot -Force) {
        if ($item.Name -notin @('TFusion.Kernel.Native.dll', 'TKernel.dll')) {
            Copy-Item -LiteralPath $item.FullName -Destination $missingBridgePackage -Recurse
        }
    }

    $failureOutput = Join-Path $workingDirectory 'missing-bridge-output.json'
    $failureProcess = Start-Process `
        -FilePath (Join-Path $missingBridgePackage 'TFusion.Diagnostics.exe') `
        -ArgumentList '--self-test', '--format', 'json' `
        -NoNewWindow -Wait -PassThru -RedirectStandardOutput $failureOutput
    $failureJson = Get-Content -LiteralPath $failureOutput -Raw
    if ($failureProcess.ExitCode -eq 0) { throw 'Diagnostics returned success when its native bridge was absent.' }
    $failureReport = $failureJson | ConvertFrom-Json
    if ($failureReport.status -ne 'fail'
        -or $failureReport.nativeKernel.loadStatus -ne 'load-failed'
        -or $failureReport.nativeKernel.diagnosticCode -ne 'TFN-KRN-LOAD') {
        throw "Missing-bridge diagnostics were not structured correctly: $failureJson"
    }
    $failureJson | Out-File `
        -LiteralPath (Join-Path $repositoryRoot 'artifacts/diagnostics-missing-bridge-test.json') `
        -Encoding utf8NoBOM
}
finally {
    $env:PATH = $originalPath
    if (Test-Path -LiteralPath $workingDirectory) {
        Remove-Item -LiteralPath $workingDirectory -Recurse -Force
    }
}
