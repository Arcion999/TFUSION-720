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

The following families are brought in by the direct packages and use their upstream license files. Exact resolved versions and hashes are recorded in each project lock file.

- Microsoft.Extensions.Configuration and its Abstractions, Binder, CommandLine, EnvironmentVariables, FileExtensions, FileProviders, Ini, Json, and UserSecrets packages — MIT, dotnet/runtime
- Microsoft.Extensions.DependencyInjection and Abstractions — MIT, dotnet/runtime
- Microsoft.Extensions.Diagnostics and Abstractions — MIT, dotnet/runtime
- Microsoft.Extensions.FileProviders.Abstractions and Physical — MIT, dotnet/runtime
- Microsoft.Extensions.FileSystemGlobbing — MIT, dotnet/runtime
- Microsoft.Extensions.Hosting.Abstractions — MIT, dotnet/runtime
- Microsoft.Extensions.Logging and Abstractions, Configuration, Console, Debug, and EventSource — MIT, dotnet/runtime
- Microsoft.Extensions.Options and ConfigurationExtensions — MIT, dotnet/runtime
- Microsoft.Extensions.Primitives — MIT, dotnet/runtime
- Microsoft.Testing.Extensions.Telemetry, TrxReport.Abstractions, VSTestBridge, and Microsoft.Testing.Platform — MIT, microsoft/testfx and microsoft/vstest
- Microsoft.TestPlatform.ObjectModel and TestHost — MIT, microsoft/vstest
- Microsoft.CodeCoverage — MIT, microsoft/vstest
- Newtonsoft.Json — MIT, JamesNK/Newtonsoft.Json
- Serilog.Extensions.Logging — Apache-2.0, serilog/serilog-extensions-logging
- System.Diagnostics.DiagnosticSource — MIT, dotnet/runtime
- System.Text.Encodings.Web and System.Text.Json — MIT, dotnet/runtime
- xunit.abstractions — Apache-2.0, xunit/xunit
- xunit.v3.assert, xunit.v3.common, xunit.v3.core, xunit.v3.extensibility.core, xunit.v3.runner.common, xunit.v3.runner.inproc.console, xunit.v3.runner.utility, and xunit.v3.runner.visualstudio.testadapter — Apache-2.0, xunit/xunit

Copyright notices and full license texts remain in their NuGet packages and upstream repositories. No third-party source code has been copied into this repository.
