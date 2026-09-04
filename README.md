# TFUSION-720

TFUSION-720 is a precision-oriented, offline-first Windows 3D CAD engineering project. It is **pre-alpha**. Milestone 1 contains no CAD functionality and makes no geometry, modelling, rendering, manufacturing, import, export, or file-format support claim. There is **no file-format support** in Milestone 1.

The present executable is deliberately limited to an honest engineering-foundation window. It has no modelling controls. Exact CAD behavior begins only after each preceding milestone has passed its mandatory gate.

## Engineering priorities

Work is governed, in order, by user data safety, geometric correctness, numerical robustness, document integrity, stability, parametric reliability, import/export reliability, manufacturing validity, UI responsiveness, performance, usability, visual polish, advanced features, and feature count.

The authoritative architecture requires C#/.NET and WPF, a narrow native C ABI to Open CASCADE Technology, exact B-Rep/NURBS geometry separate from disposable rendering meshes, Direct3D through Vortice.Windows, a parametric history, a dedicated sketch solver, modular exchange providers, a versioned native format, and transactional modelling. Those later systems are not present in Milestone 1.

## Current milestone

Milestone 1 — repository and engineering foundation. Status: **PARTIAL until CI and all recorded Windows 11 manual checks pass**. See [the milestone report](docs/milestones/M01-foundation.md).

## Supported development baseline

- Windows 11 24H2 or newer, x64
- .NET SDK 10.0.400 (selected by `global.json`)
- PowerShell 7
- Visual Studio 2026 with **.NET desktop development**; **Desktop development with C++** is required beginning in Milestone 2, not by current C++ code

No network connection is required for normal application startup. A first restore requires access to the configured NuGet source.

## Build and test

From PowerShell 7 at the repository root:

```powershell
./eng/build.ps1
./eng/test.ps1 -NoBuild
./eng/verify.ps1
```

`verify.ps1` is the complete Milestone 1 automated gate: format verification, locked restore, Release x64 build, tests and coverage thresholds, dependency audit, and deterministic Foundation assembly comparison.

Run the machine-readable local diagnostic after building:

```powershell
./src/TFusion.Diagnostics/bin/x64/Release/net10.0-windows10.0.26100.0/win-x64/TFusion.Diagnostics.exe --self-test --format json
```

## Project documents

- [Authoritative specifications](docs/specification/)
- [Architecture decisions](docs/architecture/)
- [Milestone evidence](docs/milestones/)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)
- [Third-party notices](THIRD_PARTY_NOTICES.md)

The long-term product goal is entirely local CAD operation. Cloud accounts, telemetry, and proprietary format SDKs are not core dependencies.
