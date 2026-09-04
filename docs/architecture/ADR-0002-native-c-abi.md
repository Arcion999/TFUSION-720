# ADR-0002: Narrow native C ABI

- Status: Accepted
- Date: 2026-09-03

## Context

Open CASCADE Technology is C++, while the application is managed .NET. C++ ABI, exceptions, ownership, and object layout must not cross the process-language boundary.

## Decision

Beginning in Milestone 2, expose OCCT only through a versioned, narrow, exception-contained native C ABI using opaque validated handles, fixed-width data, UTF-8 buffers, explicit ownership, status codes, and caller-queryable diagnostics. Managed interop owns no raw OCCT pointer or type.

## Consequences

The bridge requires lifecycle, stale-handle, double-release, buffer, encoding, leak, and ABI-negotiation tests. The C interface is deliberately less convenient than direct C++/CLI but more stable and auditable.

## Rejected alternatives

Direct P/Invoke to C++ exports, C++/CLI as the primary boundary, SWIG-generated broad bindings, and exposing raw pointers are rejected for ABI and ownership risk.

## Specification links

- [Master Directive §10](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Milestone 2](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#milestone-2--occt-native-bridge-foundation)
