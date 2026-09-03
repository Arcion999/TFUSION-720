# ADR-0006: Platform, toolchain, and viewport

- Status: Accepted
- Date: 2026-09-03

## Context

The product is an offline Windows desktop CAD application. A deterministic baseline is needed before native geometry and GPU work; the renderer must integrate with WPF without making UI or GPU state authoritative.

## Decision

Milestone 1 pins .NET SDK 10.0.400, C# 14, PowerShell 7, x64, and Windows 11 24H2 build 26100 or newer. WPF owns desktop chrome. Milestone 4 will use Direct3D 11 through a pinned Vortice.Windows adapter with DXGI interop and WARP fallback, after an ADR version review. Visual Studio 2026 is the supported IDE baseline.

## Consequences

CI includes portable managed tests and a `windows-2025` x64 gate. GPU claims require explicit hardware/WARP evidence in Milestone 4. The current shell has no viewport or graphics dependency.

## Rejected alternatives

Floating SDKs, AnyCPU executable delivery, OpenGL as an unreviewed substitution, WPF 3D production rendering, and an NVIDIA-only path are rejected.

## Specification links

- [Master Directive §§6, 7, 14](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Platform and toolchain decisions](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#accepted-architectural-decisions)
