# ADR-0005: Tolerance taxonomy

- Status: Accepted
- Date: 2026-09-03

## Context

One global epsilon cannot safely govern construction, solver residuals, sewing, validation, tessellation, picking, and manufacturing acceptance. Loosening a threshold merely to pass tests hides geometric errors.

## Decision

Later geometry modules use named, unit-aware tolerance policies by purpose and provenance. Stored/imported topology tolerances, modelling tolerances, solver convergence, display deflection, UI picking distance, and manufacturing limits remain separate. Every healing/tolerance change is bounded and reported.

## Consequences

Fixtures record expected tolerance ranges and failures. Tolerance changes require review and regression evidence. Display quality never changes exact model values.

## Rejected alternatives

A magic global epsilon, unitless persisted doubles, hidden imported-tolerance inflation, and adaptive weakening until an operation succeeds are rejected.

## Specification links

- [Master Directive §§17–19](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Numerical policy](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#accepted-architectural-decisions)
