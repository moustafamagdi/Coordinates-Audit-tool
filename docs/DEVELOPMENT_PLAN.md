# Development Plan

Each milestone must remain independently buildable and testable in Autodesk Revit 2024.

## M0 - Foundation

- Revit 2024 / .NET Framework 4.8 project.
- External application registration, ribbon panel, and placeholder command.
- Local build/install scripts and initial documentation.

## M1 - Host coordinate reader

- Read Project Base Point, Survey Point, Internal Origin, Project Location, and units.
- Present raw and display-unit values in a read-only view.

## M2 - Link discovery

- Enumerate link types and instances, including unavailable and duplicate instances.

## M3 - Linked coordinate reader

- Read coordinate data from directly loaded linked documents.

## M4 - Transform analysis

- Report translation and rotation and transform linked origins into host space.

## M5 - Comparison engine

- Select a reference model, apply tolerances, and return status with reasons.

## M6 - User interface

- WPF results window with summary, sorting, filtering, details, and refresh.

## M7 - Reporting

- CSV first, followed by formatted Excel output.

## M8 - Monitoring

- External audit settings and approved baselines with change detection.

## M9 - Stable release

- Logging, performance, test matrix, packaging, and release documentation.
