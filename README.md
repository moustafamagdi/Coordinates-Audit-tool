# Revit Coordinate Auditor

Read-only Autodesk Revit 2024 add-in for auditing model coordinates and linked-model placement.

## Current milestone

**M1 - Host Coordinate Reader**

- Revit 2024 project targeting .NET Framework 4.8.
- `Coordinate Auditor` ribbon panel and `Open Auditor` command.
- Read-only reporting for the active project's Project Base Point, Survey Point,
  Internal Origin, active Project Location, True North angle, and project units.
- PowerShell development install and uninstall scripts.
- No linked-model reading or model-modifying behavior yet.

## Requirements

- Windows 10 or 11
- Autodesk Revit 2024
- Visual Studio 2022 with **.NET desktop development**
- .NET Framework 4.8 Developer Pack
- PowerShell 5.1 or later

The project expects Revit 2024 at:

```text
C:\Program Files\Autodesk\Revit 2024
```

## Build and install

1. Open `CoordinatesAudit.sln` and build `Debug | Any CPU`.
2. Run:

```powershell
.\scripts\Install-Addin.ps1 -Configuration Debug
```

Or build and install from a Visual Studio Developer PowerShell:

```powershell
.\scripts\Install-Addin.ps1 -Configuration Debug -Build
```

Start Revit 2024, then use **Coordinates Audit > Coordinate Auditor > Open Auditor**.

The development installation is placed under:

```text
%APPDATA%\Autodesk\Revit\Addins\2024
```

## Uninstall development copy

```powershell
.\scripts\Uninstall-Addin.ps1
```

## Safety

M1 is read-only. It opens no Revit transaction and changes no document data.

## License

No license has been selected. All rights are reserved by the repository owner until a license is added.
