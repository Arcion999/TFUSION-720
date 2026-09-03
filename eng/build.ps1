[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot 'TFUSION-720.sln'

Push-Location $repositoryRoot
try {
    dotnet restore $solution --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed.' }

    dotnet build $solution --configuration Release --no-restore --property:Platform=x64
    if ($LASTEXITCODE -ne 0) { throw 'Release x64 build failed.' }
}
finally {
    Pop-Location
}
