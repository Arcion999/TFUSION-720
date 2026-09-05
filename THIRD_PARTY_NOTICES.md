# Third-Party Notices

This inventory covers Milestone 1 and Milestone 2 dependencies restored, built, or distributed with TFUSION-720. The committed NuGet `packages.lock.json` files and native vcpkg manifest/baseline are the authoritative dependency inventory. Package metadata was reviewed from upstream project metadata on 2026-09-04.

TFUSION-720 itself is licensed under Apache License 2.0; see `LICENSE`.

## Direct dependencies

| Package | Version | License | Source |
|---|---:|---|---|
| Microsoft.Extensions.Hosting | 10.0.11 | MIT | dotnet/runtime |
| Serilog | 4.4.0 | Apache-2.0 | serilog/serilog |
| Serilog.Extensions.Hosting | 10.0.0 | Apache-2.0 | serilog/serilog-extensions-hosting |
| Serilog.Sinks.File | 7.0.0 | Apache-2.0 | serilog/serilog-sinks-file |
| xunit.v3 | 4.0.0 | Apache-2.0 | xunit/xunit |
| Microsoft.NET.Test.Sdk | 18.9.0 | MIT | microsoft/vstest |
| Microsoft.Testing.Extensions.CodeCoverage | 18.1.0 | Microsoft .NET Library license (free-to-use package terms) | microsoft/vstest / Microsoft testing platform code coverage |

## Resolved transitive dependencies

Exact resolved versions and content hashes are recorded in each project lock file. These identifiers are repeated explicitly so automated inventory checks cannot hide a newly resolved dependency behind a family name.

The following are MIT-licensed Microsoft packages from `dotnet/runtime`, `microsoft/vstest`, `microsoft/testfx`, or `microsoft/ApplicationInsights-dotnet` as applicable:

- `Microsoft.ApplicationInsights`
- `Microsoft.Bcl.AsyncInterfaces`
- `Microsoft.CodeCoverage`
- `Microsoft.DiaSymReader`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Configuration.Binder`
- `Microsoft.Extensions.Configuration.CommandLine`
- `Microsoft.Extensions.Configuration.EnvironmentVariables`
- `Microsoft.Extensions.Configuration.FileExtensions`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Configuration.UserSecrets`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.DependencyModel`
- `Microsoft.Extensions.Diagnostics`
- `Microsoft.Extensions.Diagnostics.Abstractions`
- `Microsoft.Extensions.FileProviders.Abstractions`
- `Microsoft.Extensions.FileProviders.Physical`
- `Microsoft.Extensions.FileSystemGlobbing`
- `Microsoft.Extensions.Hosting.Abstractions`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Logging.Configuration`
- `Microsoft.Extensions.Logging.Console`
- `Microsoft.Extensions.Logging.Debug`
- `Microsoft.Extensions.Logging.EventLog`
- `Microsoft.Extensions.Logging.EventSource`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- `Microsoft.Extensions.Primitives`
- `Microsoft.Testing.Extensions.Telemetry`
- `Microsoft.Testing.Extensions.TrxReport.Abstractions`
- `Microsoft.Testing.Platform`
- `Microsoft.Testing.Platform.MSBuild`
- `Microsoft.TestPlatform.ObjectModel`
- `Microsoft.TestPlatform.TestHost`
- `Microsoft.Win32.Registry`
- `System.Security.AccessControl`

The following are Apache-2.0-licensed Serilog/xUnit packages from their respective upstream repositories:

- `Serilog.Extensions.Logging`
- `xunit.analyzers`
- `xunit.v3.assert`
- `xunit.v3.common`
- `xunit.v3.core.mtp-v2`
- `xunit.v3.extensibility.core`
- `xunit.v3.mtp-v2`
- `xunit.v3.runner.common`
- `xunit.v3.runner.inproc.console`

Copyright notices and full license texts remain in their NuGet packages and upstream repositories. No third-party source code has been copied into this repository.

## Native CAD kernel dependency

| Component | Version | Use | License | Source |
|---|---:|---|---|---|
| Open CASCADE Technology (OCCT) | 8.0.1 | Dynamically linked exact CAD kernel runtime (`TKernel.dll`) behind `TFusion.Kernel.Native.dll` | LGPL-2.1-only WITH OCCT-exception-1.0 | Open-Cascade-SAS/OCCT tag `V8_0_1` |
| vcpkg | baseline `04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4` | Build-time dependency acquisition; not an application runtime dependency | MIT | microsoft/vcpkg |

OCCT is acquired in manifest mode for the explicit `x64-windows` triplet. The manifest disables optional default features and overrides the OCCT port to exactly 8.0.1. TFUSION does not copy OCCT source code into this repository and does not expose OCCT C++ types in its public or managed interfaces.

The packaged runtime includes the vcpkg-installed OCCT copyright and full license/exception text as `THIRD_PARTY-LICENSES/OCCT.txt`. TFUSION links OCCT dynamically and retains the recipients' ability to replace the compatible LGPL library. Microsoft/Windows system DLLs and the Visual C++ platform runtime remain normal platform prerequisites.

OCCT 8.0.1 is the latest upstream serviced release checked for this milestone (released 2026-07-30). No applicable published security advisory was identified in the upstream release/security pages or the NVD search performed on 2026-09-04. This is a dated dependency review, not a guarantee that no undisclosed issue exists; automated dependency and scheduled security checks remain required.
