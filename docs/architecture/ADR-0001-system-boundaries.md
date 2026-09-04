# ADR-0001: System boundaries

- Status: Accepted
- Date: 2026-09-03

## Context

TFUSION-720 must evolve into a precise offline CAD system without coupling UI, document intent, exact geometry, tessellation, constraints, persistence, or exchange providers. Milestone 1 must establish dependencies without placeholder assemblies.

## Decision

Use C#/.NET for managed application and domain services, WPF only at the desktop composition boundary, and independently testable modules added when they first contain real behavior. Dependency direction is UI/providers → application/document contracts → kernel/render/solver adapters; infrastructure never mutates document state behind application transactions. Foundation remains portable and dependency-minimal.

## Consequences

Architecture tests enforce current references. Later milestones add narrowly responsible projects only with tested behavior. Cross-boundary data is explicit and versionable.

## Rejected alternatives

A monolithic WPF assembly, empty future project scaffolding, cloud-backed core services, and a plugin-free giant importer are rejected because they weaken testability, offline operation, and integrity.

## Specification links

- [Master Directive §§7, 12, 13, 35](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Architecture Assessment](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#architecture-assessment)
