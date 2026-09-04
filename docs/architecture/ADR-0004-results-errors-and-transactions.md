# ADR-0004: Results, errors, and transactions

- Status: Accepted
- Date: 2026-09-03

## Context

Geometry and storage failures are expected engineering outcomes. Nulls, swallowed exceptions, and partial mutation can corrupt a parametric document or conceal a wrong model.

## Decision

Expected operations return immutable `Result`/`Result<T>` values with stable diagnostic codes, severity, user-safe text, separate technical detail, immutable context, and causal order. From Milestone 5, modelling computes against candidate state; only a validated result commits atomically. Failure retains the last valid document and geometry.

## Consequences

Failure paths require tests and visible diagnostics. Exceptions remain appropriate for programmer contract violations and are contained/logged at process and native boundaries.

## Rejected alternatives

Null-as-failure, Boolean-only status, catch-and-ignore, mutating live document state before validation, and automatically accepting partial results are rejected.

## Specification links

- [Master Directive §§15, 16, 22](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Result and diagnostic contracts](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#result-and-diagnostic-contracts)
