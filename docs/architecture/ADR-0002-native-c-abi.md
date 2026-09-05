# ADR-0002: Narrow native C ABI

- Status: Accepted
- Date: 2026-09-03

## Context

Open CASCADE Technology is C++, while the application is managed .NET. C++ ABI, exceptions, ownership, and object layout must not cross the process-language boundary.

## Decision

Beginning in Milestone 2, expose OCCT only through a versioned, narrow, exception-contained native C ABI using opaque validated handles, fixed-width data, UTF-8 buffers, explicit ownership, status codes, and caller-queryable diagnostics. Managed interop owns no raw OCCT pointer or type.

ABI version 1 uses `__cdecl`, fixed-width C99 integer types, caller-sized/versioned structures, caller-owned UTF-8 buffers, and opaque 64-bit values backed by a generation-checked registry. Handles are not native addresses. A kernel context owns all child handles; destroying it invalidates them. Managed ownership is expressed through dedicated `SafeHandle` types in `TFusion.Kernel.Interop`.

Every exported function is enclosed by the same defensive boundary, which converts OCCT `Standard_Failure`, `std::exception`, and unknown C++ exceptions to stable status values plus a context or thread diagnostic. OCCT remains dynamically linked and isolated inside `TFusion.Kernel.Native.dll`.

## Consequences

The bridge requires lifecycle, stale-handle, double-release, buffer, encoding, leak, and ABI-negotiation tests. The C interface is deliberately less convenient than direct C++/CLI but more stable and auditable.

Milestone 2 uses OCCT 8.0.1 and vcpkg manifest mode at pinned baseline `04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4`, with the explicit `x64-windows` triplet. This implements the already-approved dependency approach; it is not an architecture deviation.

## Rejected alternatives

Direct P/Invoke to C++ exports, C++/CLI as the primary boundary, SWIG-generated broad bindings, and exposing raw pointers are rejected for ABI and ownership risk.

## Specification links

- [Master Directive §10](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Milestone 2](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#milestone-2--occt-native-bridge-foundation)
