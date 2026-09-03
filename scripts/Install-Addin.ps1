[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [string]$RevitInstallDir = "C:\Program Files\Autodesk\Revit 2024",
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\CoordinatesAudit\CoordinatesAudit.csproj"
$outputDirectory = Join-Path $repositoryRoot "src\CoordinatesAudit\bin\$Configuration\net48"
$sourceAssembly = Join-Path $outputDirectory "CoordinatesAudit.dll"

if ($Build) {
    & dotnet build $projectPath --configuration $Configuration -p:RevitInstallDir="$RevitInstallDir"
    if ($LASTEXITCODE -ne 0) { throw "The project build failed." }
}

if (-not (Test-Path $sourceAssembly)) {
    throw "CoordinatesAudit.dll was not found. Build first or run this script with -Build."
}

$revitAddinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2024"
$deploymentDirectory = Join-Path $revitAddinsRoot "CoordinatesAudit"
$manifestDestination = Join-Path $revitAddinsRoot "CoordinatesAudit.addin"
$manifestTemplate = Join-Path $repositoryRoot "manifests\CoordinatesAudit.addin.template"
$deployedAssembly = Join-Path $deploymentDirectory "CoordinatesAudit.dll"

New-Item -ItemType Directory -Path $deploymentDirectory -Force | Out-Null
Copy-Item $sourceAssembly $deployedAssembly -Force
$sourcePdb = Join-Path $outputDirectory "CoordinatesAudit.pdb"
if (Test-Path $sourcePdb) { Copy-Item $sourcePdb $deploymentDirectory -Force }

$escapedAssemblyPath = [System.Security.SecurityElement]::Escape($deployedAssembly)
$manifestContent = (Get-Content $manifestTemplate -Raw).Replace("__ASSEMBLY_PATH__", $escapedAssemblyPath)
Set-Content -Path $manifestDestination -Value $manifestContent -Encoding UTF8

Write-Host "Coordinate Auditor installed for Revit 2024."
Write-Host "Manifest: $manifestDestination"
Write-Host "Assembly: $deployedAssembly"
