# Milestone 2 — OCCT native bridge foundation

## MILESTONE STATUS

**Status: PARTIAL — implementation is in review; the mandatory human clean-machine gate has not been performed.**

Milestone 3 is blocked. This report must not be changed to `PASS` until every automated gate is green on the final candidate and the human evidence table below is completed.

## Objective

Build and package a real `TFusion.Kernel.Native.dll` backed by Open CASCADE Technology, then expose it to managed .NET through the narrow, versioned C ABI required by ADR-0002. This milestone implements kernel-boundary infrastructure only. It does not create, display, import, export, or persist CAD geometry.

## Implementation summary

- `TFusion.Kernel.Native.dll` is a project-owned C++17 DLL linked dynamically to OCCT `TKernel.dll`.
- ABI v1 is defined in a C-compatible public header and uses `__cdecl`, fixed-width integer values, sized/versioned structures, caller-owned UTF-8 buffers, and stable statuses.
- Native contexts are explicit. Context shutdown invalidates every context-owned child handle.
- Opaque 64-bit handles use slot and generation validation; they are never native addresses.
- Every exported entry point uses a shared exception boundary for OCCT `Standard_Failure`, `std::exception`, and unknown exceptions.
- Native failures produce stable status codes and detailed UTF-8 JSON diagnostics scoped to the originating context when it is valid, with a thread-local diagnostic for failures that have no valid context.
- `TFusion.Kernel.Interop` owns all P/Invoke declarations and maps failures to the Milestone 1 `Result`/`CadDiagnostic` model.
- Managed context and probe ownership uses `SafeHandle`; deterministic `Dispose` is primary and SafeHandle finalization is the fallback.
- `TFusion.Diagnostics` reports the real ABI, compiled OCCT version, runtime OCCT version, architecture, load location, and initialization outcome.
- Native binaries and actual runtime dependencies are copied into managed/package outputs by the normal build. The loader accepts the bridge only from `AppContext.BaseDirectory`.

## Exact dependency and toolchain baseline

| Item | Pinned value |
|---|---|
| OCCT | 8.0.1 (`V8_0_1`) |
| vcpkg baseline | `04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4` |
| vcpkg triplet | `x64-windows` |
| vcpkg OCCT default features | disabled |
| Native language | C++17; C17 ABI smoke consumer |
| Native build | CMake 3.28+, Ninja, supported Visual Studio MSVC x64 toolchain |
| Managed runtime | .NET SDK 10.0.400; `net10.0-windows10.0.26100.0`; `win-x64` |
| ABI version | 1 |
| Calling convention | `__cdecl` |

The OCCT baseline was rechecked at milestone start. 8.0.1 was the latest serviced upstream release (2026-07-30), retains the 8.0 C++17 API/ABI baseline, and includes maintenance reliability fixes. No applicable published advisory was identified in the checked upstream security/release information and NVD search on 2026-09-04. No version deviation was required.

## Modules and files changed

- `native/TFusion.Kernel.Native/`: vcpkg manifest, CMake build, C ABI, context, diagnostics, UTF-8 validation, handle registry, native/C tests.
- `src/TFusion.Kernel.Interop/`: native loader, exact managed ABI structures, P/Invoke declarations, SafeHandles, typed managed facade, diagnostic mapping.
- `src/TFusion.Foundation/Diagnostics/KernelDiagnosticCodes.cs`: stable managed kernel diagnostic codes.
- `src/TFusion.Diagnostics/`: truthful kernel self-test and machine-readable native report.
- `tests/TFusion.Kernel.Tests/`: managed ABI, ownership, error, UTF-8, load, and lifecycle coverage.
- `tests/TFusion.Architecture.Tests/`: enforced dependency, isolation, C ABI, exception-boundary, and milestone-scope policies.
- `eng/`: pinned vcpkg acquisition, native build/test/install, packaged diagnostics load test, integrated full verification.
- `.github/workflows/`: Windows native/managed gate, AddressSanitizer run, C# CodeQL, and C++ CodeQL.
- `THIRD_PARTY_NOTICES.md`: OCCT/vcpkg licensing, provenance, and redistribution record.

## Automated acceptance gate

| Gate | Coverage | State / evidence |
|---|---|---|
| ABI negotiation | query v1; accept v1; reject zero, future, and unsupported versions | PENDING CI |
| Context lifecycle | create/use/destroy; invalid and repeated destroy; child cleanup | PENDING CI |
| Handle validation | valid, random, invalid, stale, released, double release, type/context mismatch | PENDING CI |
| Exception containment | OCCT, standard, and unknown C++ exception probes; diagnostic retrieval | PENDING CI |
| UTF-8 and buffers | ASCII, non-ASCII, malformed input, length query, undersized buffer, termination | PENDING CI |
| Native lifecycle stress | 10,000 context + 10,000 probe allocations and deterministic releases | PENDING CI |
| Native instrumentation | MSVC AddressSanitizer build/test of project-owned bridge and test executables | PENDING CI |
| Project-owned leak evidence | registry active-context/probe counters return to zero; allocation/release deltas match 20,000 | PENDING CI |
| Managed ownership | deterministic disposal, repeat disposal, owner lifetime, SafeHandle finalizer fallback | PENDING CI |
| Packaging/load | publish output runs from unrelated working directory with `PATH` limited to Windows system paths | PENDING CI |
| Real OCCT execution | compiled macro and runtime `OCCT_Version_String_Complete()` report pinned runtime; runtime probe matches | PENDING CI |
| Milestone 1 regression | Foundation and architecture tests, coverage, determinism, formatting, locked restore, audit | PENDING CI |
| Security | C# CodeQL and C++ CodeQL | PENDING CI |

MSVC AddressSanitizer is used for out-of-bounds, use-after-free, and related memory-safety instrumentation of project-owned native code. It is not represented as a universal proof that OCCT or every process allocation is leak-free. Leak/lifetime evidence for this milestone is the explicit native registry accounting across the required 10,000-iteration stress gate plus sanitizer success.

## Manual acceptance test — human execution required

### Preconditions

- Clean supported Windows 11 x64 environment (24H2/build 26100 or newer).
- Supported Visual Studio C++ and .NET workloads, Git, PowerShell 7, CMake, and Ninja.
- No manually installed OCCT directory on `PATH` and no copied TFUSION/OCCT DLLs in system directories.
- A clean clone at the final Milestone 2 candidate SHA.

### Procedure

1. In PowerShell 7, record `git rev-parse HEAD`, `[Environment]::OSVersion.Version`, and `$env:PATH`.
2. Run `./eng/verify.ps1`. Do not copy any DLL, add any path, or set an undocumented variable.
3. Change to a new empty temporary working directory.
4. Run the absolute path to `artifacts/diagnostics-package/TFusion.Diagnostics.exe --self-test --format json`.
5. Verify exit code `0` and JSON values: `status=pass`, `nativeKernel.loadStatus=loaded`, `abiVersion=1`, both OCCT versions contain `8.0.1`, `architecture=x64`, and `initializationResult=success`.
6. Confirm the reported `nativeBridgePath` is inside the diagnostics package and not a developer/system directory.
7. Attach the complete command output and JSON, identify the human tester, and record the date and tested commit below.

### Human evidence

| Field | Evidence |
|---|---|
| Tester | NOT YET SUPPLIED |
| Date | NOT YET SUPPLIED |
| Windows build | NOT YET SUPPLIED |
| Tested commit | NOT YET SUPPLIED |
| `eng/verify.ps1` | NOT YET SUPPLIED |
| Packaged diagnostics JSON | NOT YET SUPPLIED |
| No manual DLL/PATH change | NOT YET SUPPLIED |

## Known limitations and technical debt

- The human clean-machine gate is outstanding, so Milestone 2 remains `PARTIAL` even after automated CI succeeds.
- ABI v1 contains diagnostics and lifecycle capabilities only. All geometry operations correctly remain absent rather than returning placeholder success.
- Windows MSVC AddressSanitizer does not by itself establish that every third-party allocation is leak-free; the report distinguishes sanitizer findings from project registry lifetime accounting.
- The native bridge is Windows x64 only, matching the approved product/platform baseline.

## Architecture deviations

None. The implementation refines accepted ADR-0002 without changing the required C# application ownership, OCCT exact-geometry ownership, GPU visualization boundary, or format-provider boundary.

## Licensing and provenance

OCCT is integrated through the pinned official vcpkg port and dynamically linked. No FreeCAD, CADability, PlaneGCS, OCCT example, or other third-party implementation source was copied. The packaged OCCT copyright/license/exception record is installed from vcpkg with the native runtime. Full details are in `THIRD_PARTY_NOTICES.md`.

## Scope stop

Milestone 3 has not started. There are no primitives, B-Rep feature operations, sketches, renderer, tessellation, picking, persistence, or import/export providers in this change.
