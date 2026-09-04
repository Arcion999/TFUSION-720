[CmdletBinding()]
param(
    [switch]$AddressSanitizer,
    [switch]$SkipTests,
    [switch]$SkipInstall
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'The OCCT native bridge can only be built on Windows x64.'
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$nativeRoot = Join-Path $repositoryRoot 'native/TFusion.Kernel.Native'
$preset = if ($AddressSanitizer) { 'windows-x64-asan' } else { 'windows-x64-release' }
$buildRoot = Join-Path $repositoryRoot "artifacts/native-build/$(if ($AddressSanitizer) { 'asan' } else { 'release' })"
$packageRoot = Join-Path $repositoryRoot 'artifacts/native/package'

& (Join-Path $PSScriptRoot 'acquire-vcpkg.ps1')
$env:VCPKG_DISABLE_METRICS = '1'
if (-not [string]::IsNullOrWhiteSpace($env:TFUSION_VCPKG_BINARY_CACHE)) {
    New-Item -ItemType Directory -Force -Path $env:TFUSION_VCPKG_BINARY_CACHE | Out-Null
    $env:VCPKG_BINARY_SOURCES = "clear;files,$env:TFUSION_VCPKG_BINARY_CACHE,readwrite"
}

Push-Location $nativeRoot
try {
    cmake --preset $preset
    if ($LASTEXITCODE -ne 0) { throw "Native CMake configuration failed for $preset." }

    cmake --build --preset $preset
    if ($LASTEXITCODE -ne 0) { throw "Native build failed for $preset." }

    if (-not $SkipTests) {
        ctest --preset $preset
        if ($LASTEXITCODE -ne 0) { throw "Native tests failed for $preset." }
    }

    if (-not $SkipInstall) {
        if ($AddressSanitizer) {
            throw 'AddressSanitizer builds are test-only and must not be installed as the distributable runtime.'
        }

        New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
        cmake --install $buildRoot --prefix $packageRoot
        if ($LASTEXITCODE -ne 0) { throw 'Native runtime installation failed.' }

        foreach ($requiredFile in @('TFusion.Kernel.Native.dll', 'TKernel.dll', 'THIRD_PARTY-LICENSES/OCCT.txt')) {
            if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $requiredFile) -PathType Leaf)) {
                throw "Native package is incomplete: $requiredFile is missing."
            }
        }

        $inventory = [ordered]@{
            schemaVersion = 1
            bridgeAbiVersion = 1
            occtVersion = '8.0.1'
            vcpkgBaseline = '04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4'
            triplet = 'x64-windows'
            configuration = 'Release'
        }
        $inventory | ConvertTo-Json | Out-File `
            -LiteralPath (Join-Path $packageRoot 'native-dependency-inventory.json') `
            -Encoding utf8NoBOM
    }
}
finally {
    Pop-Location
}
