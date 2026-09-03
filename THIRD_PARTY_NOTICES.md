# Third-Party Notices

This inventory covers Milestone 1 dependencies restored or distributed with TFUSION-720. The committed `packages.lock.json` files are the authoritative resolved-version inventory; architecture test M1-A12 fails when a locked package name is absent here. Package metadata was reviewed from the upstream project and NuGet package metadata on 2026-09-03.

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
| coverlet.collector | 10.0.1 | MIT | coverlet-coverage/coverlet |

## Resolved transitive dependencies

Exact resolved versions and content hashes are recorded in each project lock file. These identifiers are repeated explicitly so automated inventory checks cannot hide a newly resolved dependency behind a family name.

The following are MIT-licensed Microsoft packages from `dotnet/runtime`, `microsoft/vstest`, `microsoft/testfx`, or `microsoft/ApplicationInsights-dotnet` as applicable:

- `Microsoft.ApplicationInsights`
- `Microsoft.Bcl.AsyncInterfaces`
- `Microsoft.CodeCoverage`
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
