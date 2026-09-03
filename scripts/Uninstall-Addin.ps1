[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$revitAddinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2024"
$deploymentDirectory = Join-Path $revitAddinsRoot "CoordinatesAudit"
$manifestPath = Join-Path $revitAddinsRoot "CoordinatesAudit.addin"

if (Test-Path $manifestPath) { Remove-Item $manifestPath -Force }
if (Test-Path $deploymentDirectory) { Remove-Item $deploymentDirectory -Recurse -Force }
Write-Host "Coordinate Auditor development installation removed."
