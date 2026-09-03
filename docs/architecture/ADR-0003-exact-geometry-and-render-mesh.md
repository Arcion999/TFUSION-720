# ADR-0003: Exact geometry and render meshes

- Status: Accepted
- Date: 2026-09-03

## Context

Manufacturing CAD needs analytic B-Rep/NURBS authority, while GPUs render triangles. Treating tessellation as the model would destroy geometric meaning and precision.

## Decision

OCCT B-Rep/NURBS shapes are the sole exact geometric authority. Versioned tessellation is disposable visualization data derived from exact shapes. Selection IDs map render primitives back to stable semantic/topological references; arrays of faces or edges are never the only persistent identity.

## Consequences

Meshes can be invalidated and regenerated without changing the document. Measurement, validation, operations, save, and STEP exchange operate on exact geometry. Rendering never receives a document mutation API.

## Rejected alternatives

Triangle-mesh modelling, WPF 3D as the production renderer, serializing GPU buffers as geometry, and topology-array-index persistence are rejected.

## Specification links

- [Master Directive §§8, 9, 14, 21](../specification/MASTER_IMPLEMENTATION_DIRECTIVE.md)
- [Roadmap: Milestones 3–4](../specification/TFUSION-720_Definitive_Milestone_Roadmap.md#milestone-3--exact-primitive-geometry-and-kernel-validation)
