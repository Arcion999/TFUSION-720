[CmdletBinding()]
param(
    [switch]$AddressSanitizer,
    [switch]$SkipTests,
    [switch]$SkipInstall,
    [switch]$Rebuild,
    [switch]$ConfigureOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'The OCCT native bridge can only be built on Windows x64.'
}

function Initialize-MsvcEnvironment {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere -PathType Leaf)) {
        throw 'Visual Studio Installer vswhere.exe was not found.'
    }

    $installationPath = & $vswhere `
        -latest `
        -products '*' `
        -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
        -property installationPath | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($installationPath)) {
        throw 'A supported Visual Studio installation with the MSVC x64 toolchain was not found.'
    }
    $installationPath = $installationPath.Trim()

    $vcvars = Join-Path $installationPath 'VC/Auxiliary/Build/vcvars64.bat'
    if (-not (Test-Path -LiteralPath $vcvars -PathType Leaf)) {
        throw "The MSVC x64 environment script was not found: $vcvars"
    }

    $environmentLines = & $env:ComSpec /d /s /c "`"$vcvars`" >nul && set"
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to initialize the MSVC x64 developer environment.'
    }

    foreach ($line in $environmentLines) {
        $separator = $line.IndexOf('=')
        if ($separator -le 0) {
            continue
        }

        $name = $line.Substring(0, $separator)
        $value = $line.Substring($separator + 1)
        Set-Item -Path "Env:$name" -Value $value
    }

    $compiler = Get-Command cl.exe -CommandType Application -ErrorAction SilentlyContinue
    if ($null -eq $compiler) {
        throw 'MSVC cl.exe was not available after developer-environment initialization.'
    }
    Write-Host "MSVC x64 compiler: $($compiler.Source)"
}

Initialize-MsvcEnvironment

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

    if ($ConfigureOnly) {
        Write-Host "Native dependencies and CMake configuration are ready for $preset."
        return
    }

    if ($Rebuild) {
        cmake --build --preset $preset --target clean
        if ($LASTEXITCODE -ne 0) { throw "Native clean rebuild preparation failed for $preset." }
    }

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
