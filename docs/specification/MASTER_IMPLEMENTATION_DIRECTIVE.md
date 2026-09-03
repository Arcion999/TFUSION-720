# MASTER IMPLEMENTATION DIRECTIVE FOR THE CAD APPLICATION

## 1. Purpose of this document

This text must be read together with the accompanying deep-research document.

The deep-research document is the primary technical specification for this project. It contains the intended architecture, technology choices, implementation order, precision strategy, CAD-kernel strategy, rendering strategy, file-format strategy, UI direction, testing philosophy, manufacturing validation approach, stability requirements, and future roadmap.

This Master Implementation Directive defines **how you, the coding agent, must interpret and execute that specification**.

Do not treat the deep-research document as a collection of optional ideas.

Treat it as the authoritative engineering direction for the application unless a requirement is technically impossible, legally impossible, incompatible with another requirement, or demonstrably unsafe for the architecture.

If such a conflict is discovered, document it clearly before changing direction.

---

# 2. The application we are building

The objective is to create a serious, offline, Windows-based, precision 3D CAD application primarily written in C#.

The application should provide a workflow familiar to users of modern parametric CAD applications such as Autodesk Fusion, while remaining an original product with its own implementation, identity, visual assets, source code, native file format, and architecture.

The program is primarily a:

**Precision parametric mechanical 3D CAD modeller.**

The main objective is NOT to recreate every Fusion product area.

The priority is:

1. Accurate 2D sketching.
2. Geometric and dimensional constraints.
3. Parametric modelling.
4. Exact B-Rep solid geometry.
5. Surface modelling.
6. Reliable feature history.
7. High-quality interaction and selection.
8. Accurate measurement.
9. Reliable import/export.
10. Manufacturing-oriented geometry validation.
11. Stability.
12. Performance.
13. Large-model usability.
14. Customizable professional desktop UI.

Features such as:

- CAM,
- FEA,
- simulation,
- PCB design,
- generative design,
- cloud collaboration,

are NOT part of the first core objective.

They may become later modules.

Do not allow those future areas to distract from building an excellent CAD modeller first.

---

# 3. The intended user experience

A user should eventually be able to:

1. Launch the Windows EXE.
2. Create a new document.
3. Select a plane.
4. Create a sketch.
5. Draw lines, circles, arcs, splines, rectangles, slots, polygons and construction geometry.
6. Add geometric constraints.
7. Add exact dimensions.
8. Define named parameters and mathematical expressions.
9. Fully constrain the sketch.
10. Finish the sketch.
11. Extrude, revolve, sweep or loft geometry.
12. Create holes.
13. Add fillets and chamfers.
14. Shell bodies.
15. Apply draft.
16. Mirror geometry.
17. Create rectangular and circular patterns.
18. Split or combine bodies.
19. Create construction planes, axes and coordinate systems.
20. Modify an earlier sketch or parameter.
21. Have dependent geometry recompute correctly.
22. Select faces, edges, vertices, bodies and components.
23. Measure the resulting geometry accurately.
24. Inspect model validity and tolerances.
25. Save the project in the application's own native parametric format.
26. Close the program.
27. Reopen the project without losing design intent.
28. Import existing CAD models.
29. Modify supported imported geometry.
30. Export accurate manufacturing-compatible files such as STEP.
31. Export mesh files such as STL or 3MF.
32. Validate exported geometry.
33. Use the resulting exported files in external CAD, CAM, CNC or additive-manufacturing software.

This workflow is the central success criterion for the project.

---

# 4. Core architectural rule

The following architectural principle is mandatory:

**C# owns the application.**
**The CAD kernel owns exact geometry.**
**The GPU owns visualization.**
**Format providers own translation.**

These responsibilities must remain separated.

The intended architecture is conceptually:

```text
C# Application
│
├── UI
├── Commands
├── Documents
├── Parameters
├── Sketches
├── Feature History
├── Assemblies
├── Undo / Redo
├── Persistence
├── Settings
└── Import / Export abstraction
        │
        ├─────────────── Rendering
        │                    │
        │                    ▼
        │              Direct3D / GPU
        │
        ▼
Native Kernel Bridge
        │
        ▼
Open CASCADE
        │
        ├── B-Rep
        ├── NURBS
        ├── exact curves
        ├── exact surfaces
        ├── booleans
        ├── fillets
        ├── chamfers
        ├── intersections
        ├── healing
        └── validation

```

Do not collapse these systems into one large project.

Do not expose the entire Open CASCADE C++ API directly to managed C#.

Use a narrow and controlled native interface.

---

# 5. Exact geometry is the source of truth

The CAD model is NOT a triangle mesh.

This is a non-negotiable requirement.

Internally:

```text
Exact B-Rep / analytic / NURBS model
              ↓
       Tessellation
              ↓
      Rendering mesh
              ↓
            GPU

```

Triangles generated for visualization must never become the authoritative representation of ordinary CAD solids.

A cylinder must remain an actual cylindrical CAD surface where possible.

A plane must remain an actual plane.

A circle must remain an exact curve.

NURBS surfaces must remain mathematical surfaces.

The display mesh is disposable and regeneratable.

The exact CAD model is authoritative.

---

# 6. Precision has priority over visual appearance

A beautiful viewport with unreliable geometry is considered a failed CAD application.

Geometrical correctness must take priority over:

- animations,
- fancy materials,
- visual effects,
- PBR,
- shadows,
- decorative UI,
- marketing-style presentation.

Rendering quality should become excellent, but only after the geometric foundation is reliable.

The application is intended to create models that may eventually be manufactured.

Do not knowingly sacrifice geometric correctness for convenience.

---

# 7. Never use one global tolerance

Do not introduce a universal value such as:

```csharp
const double EPSILON = 0.001;

```

and use it indiscriminately throughout the program.

Different systems require different tolerance concepts.

Maintain separate policies for, at minimum:

- modelling linear tolerance,
- modelling angular tolerance,
- sketch constraint tolerance,
- coincidence tolerance,
- intersection tolerance,
- sewing tolerance,
- imported geometry tolerance,
- maximum healing tolerance,
- tessellation chord tolerance,
- tessellation angular tolerance,
- snapping tolerance,
- picking tolerance,
- exported geometry tolerance.

Tolerance handling must be context-aware and scale-aware.

Do not fix geometry failures simply by repeatedly increasing tolerances until an operation succeeds.

That can hide invalid geometry.

---

# 8. Geometry operations must be transactional

A failed modelling operation must not destroy the previous valid model.

For example:

```text
Valid Body
   ↓
Attempt Fillet
   ↓
Kernel Failure

```

The result must be:

```text
Original Body remains valid
+
Feature reports failure
+
User receives meaningful diagnostic information

```

NOT:

```text
Document corrupted

```

Each significant operation should conceptually follow:

```text
Begin transaction
      ↓
Calculate
      ↓
Validate
      ↓
Success?
 ┌────┴────┐
Yes        No
 ↓          ↓
Commit    Rollback

```

The application should be designed so failed geometry calculations are normal recoverable events.

---

# 9. Never fake geometry success

Do not implement a modelling command by showing something visually similar while the actual geometric operation is absent.

Examples of unacceptable fake implementations:

- displaying a chamfer-looking mesh without modifying the B-Rep,
- drawing a hole visually without creating the actual cut,
- showing an STL-looking surface when STEP export still contains the original body,
- reporting “Fully Constrained” without actual constraint analysis,
- returning success after a kernel operation failed,
- generating dummy imported bodies when parsing fails.

If a feature is not implemented, it must remain explicitly unsupported.

Prefer:

```text
Feature not yet implemented

```

over a feature that appears to work but produces incorrect engineering data.

---

# 10. No placeholder-driven development

Temporary placeholders are acceptable only when required to establish architecture and clearly marked as temporary.

They must not become permanent silent implementations.

Do not populate the application with:

- fake buttons,
- fake dialogs,
- TODO-only commands,
- empty service classes,
- simulated import success,
- hard-coded sample geometry pretending to be a modeller.

A button should not be presented as functional unless its command has a meaningful implementation.

---

# 11. Parametric modelling is fundamental

The application must not become only a direct-modelling viewer.

The intended document model is parametric.

A feature should contain information similar to:

```text
Feature
├── ID
├── Type
├── Name
├── Inputs
├── Parameters
├── Dependencies
├── Result Bodies
├── Suppression State
├── Validation State
└── Error State

```

The user should eventually be able to have:

```text
Sketch01
   ↓
Extrude01
   ↓
Fillet01
   ↓
Hole01
   ↓
Pattern01

```

and later modify:

```text
Sketch01.Width

```

causing dependent features to recompute.

The model must retain design intent wherever possible.

---

# 12. Topological naming must be treated as a first-class problem

Never rely only on:

```text
Edge 7
Face 3
Vertex 12

```

as persistent model references.

Topological indices can change after recomputation.

Use the topology-reference strategy described in the deep-research document.

Persistent references should incorporate information such as:

- source feature,
- generating feature,
- geometric type,
- creation relationship,
- geometric signature,
- centroid,
- normal or direction,
- radius where appropriate,
- neighboring geometry,
- kernel history,
- persistent IDs where available.

Reference resolution should use progressively weaker fallbacks.

If a reference cannot safely be recovered, report:

**Reference Lost**

and allow the user to repair it.

Never silently attach a feature to a different face merely because it has a similar index.

---

# 13. Sketch solver requirements

The sketch constraint system must be treated as its own serious subsystem.

Do not create a collection of ad-hoc geometric corrections.

Build a real constraint graph and numerical solver architecture.

The solver must eventually support:

- Coincident
- Horizontal
- Vertical
- Distance
- Horizontal distance
- Vertical distance
- Radius
- Diameter
- Parallel
- Perpendicular
- Tangent
- Equal
- Concentric
- Midpoint
- Symmetry
- Fix / Unfix

and later advanced constraints.

The solver should report:

- residual,
- convergence state,
- degrees of freedom,
- conflicting constraints,
- redundant constraints,
- under-constrained state,
- fully constrained state.

A solver failure must never crash the UI.

---

# 14. Expressions and units are part of the model

Do not store all engineering values as dimensionless numbers.

Use typed quantities.

Conceptually:

```text
10 mm
25 deg
150 mm²
500 mm³

```

are not equivalent.

Parameters should support:

- names,
- units,
- expressions,
- dependencies,
- comments,
- mathematical functions.

Examples:

```text
plateWidth = 120 mm

wallThickness = 4 mm

holeDiameter = wallThickness * 2

edgeOffset = max(10 mm, holeDiameter * 1.5)

```

Circular parameter dependencies must be detected and rejected with a clear error.

---

# 15. Display precision is not modelling precision

Showing:

```text
25.40 mm

```

on screen does not mean the internal geometry should be rounded to two decimal places.

Maintain strict separation between:

- displayed decimal precision,
- internal numerical representation,
- kernel tolerances,
- exported tolerances,
- tessellation tolerances.

Changing UI decimal display must never modify model geometry.

---

# 16. Import/export requirements

The application must be designed around modular format providers.

Do not build one giant import/export class.

Formats should be handled through a provider architecture.

Open and neutral formats should receive priority.

High-priority practical targets include:

- STEP
- IGES
- STL
- OBJ
- 3MF
- DXF
- SVG
- 3DM

STEP should be considered the primary precision-neutral engineering interchange format.

Proprietary formats such as:

- SolidWorks,
- Inventor,
- CATIA,
- Creo,
- NX,
- Parasolid,
- ACIS,
- JT,
- DWG,

must use appropriate licensed translators if robust legal support requires them.

Do not attempt to invent undocumented parsers merely to claim format support.

---

# 17. F3D / F3Z / WIRE restriction

Do NOT make reverse-engineering Fusion's proprietary formats a foundational project activity.

F3D/F3Z/WIRE support must remain capability-gated until there is a legitimate and technically reliable translator.

The absence of native F3D import must NOT delay development of the CAD application.

Users can use neutral formats such as STEP for interoperability.

Do not advertise a format as supported until actual test files have demonstrated meaningful compatibility.

---

# 18. Imported geometry must be validated

Imported CAD files can contain:

- gaps,
- high tolerances,
- malformed wires,
- tiny edges,
- invalid faces,
- non-manifold regions,
- broken shells,
- self-intersections.

Every CAD import should pass through validation.

Automatic healing must remain conservative.

Aggressive healing should require explicit user action.

Any healing that modifies geometry must be reported.

Example:

```text
IMPORT REPORT

Bodies imported:                  4

Geometry repairs:
2 wires repaired
3 vertices merged
1 gap sewn

Maximum resulting tolerance:
0.000012 mm

Rejected geometry:
1 face

Status:
Imported with warnings

```

Never silently rewrite questionable engineering geometry.

---

# 19. Manufacturing validation is a major feature

A central product objective is to provide unusually transparent model validation.

The application should eventually report information such as:

```text
MODEL HEALTH

Geometry Valid                    PASS
Closed Solid                      PASS
Manifold Topology                 PASS
Self Intersections                PASS
Open Boundaries                   0
Invalid Faces                     0
Degenerate Edges                  0
Units Defined                     PASS
Tolerance Profile                 PASS
STEP Round Trip                   PASS

MANUFACTURING READINESS

PASS

```

If the model fails:

```text
MANUFACTURING READINESS

FAIL

Reasons:

2 open edges
1 invalid face
Imported tolerance exceeds profile

```

Never make “Manufacturing Ready” a cosmetic badge.

It must be based on real validation.

---

# 20. STEP round-trip validation

For important manufacturing exports, do more than call:

```text
ExportSTEP();

```

A preferred validation flow is:

```text
Current exact model
        ↓
STEP export
        ↓
New STEP file
        ↓
Fresh independent import
        ↓
New exact model
        ↓
Comparison

```

Compare where appropriate:

- number of solids,
- component count,
- volume,
- surface area,
- bounding box,
- important dimensions,
- face count,
- validity,
- units.

This should help detect corrupt or degraded exports.

---

# 21. Native file format

The application must have its own versioned native format.

Do not make STEP the native project format.

A native document must retain information such as:

- sketches,
- constraints,
- parameters,
- expressions,
- feature history,
- bodies,
- components,
- assembly information,
- materials,
- views,
- references,
- precision settings,
- metadata.

The native format should be versioned and migration-capable.

Saving must be atomic.

Never destroy the user's only valid copy while saving a replacement.

Use temporary-file + validation + rename/replacement semantics.

---

# 22. Undo, redo and command system

User actions should be structured as transactions/commands.

Do not implement undo by randomly cloning the entire application state for every mouse click unless there is a specific justified case.

Commands should support:

- Execute
- CanExecute
- Undo
- Redo where applicable
- transaction boundaries
- structured failure

A failed command must not appear in history as a successful modelling action.

---

# 23. Stability requirements

Stability has higher priority than feature count.

A professional CAD program must expect failures from:

- geometry kernels,
- imported files,
- GPU drivers,
- malformed data,
- huge assemblies,
- memory exhaustion,
- cancelled calculations,
- unexpected topology changes.

Design defensively.

Native crashes and translator crashes should be isolated where practical.

Risky file translators should be able to operate in a separate worker process.

For example:

```text
CadApp.exe
    ↓
CadTranslatorWorker.exe
    ↓
Third-party translator

```

If the worker crashes:

```text
worker fails
≠
main CAD application fails

```

The user should receive an error rather than lose the entire editing session.

---

# 24. Autosave and crash recovery

Do not rely only on a timer that writes the entire file occasionally.

Use an architecture involving:

```text
User action
    ↓
Transaction journal
    ↓
Recovery information
    ↓
Periodic snapshots
    ↓
Normal explicit save

```

After a crash, the application should attempt to reconstruct unsaved work.

Crash recovery must be designed before the program becomes large.

---

# 25. Rendering requirements

The rendering system must be separate from the CAD kernel.

Initial production rendering should prioritize stability.

Use the rendering architecture specified in the research.

Direct3D 11 is acceptable as the first stable renderer.

Direct3D 12 can remain a future or experimental backend.

Support:

- discrete GPU selection,
- automatic high-performance adapter selection,
- NVIDIA GPUs,
- integrated GPUs,
- software fallback where practical.

The program must not require NVIDIA hardware to function.

NVIDIA acceleration should improve performance where available.

---

# 26. GPU versus CPU responsibilities

The GPU should handle visualization workloads such as:

- triangle rendering,
- vertex/index processing,
- depth buffering,
- highlighting,
- edges,
- overlays,
- anti-aliasing,
- possibly selection buffers,
- materials,
- later PBR,
- large visualization workloads.

The GPU should NOT become the authoritative exact geometry kernel.

Operations such as:

- B-Rep booleans,
- exact intersections,
- CAD topology,
- surface trimming,
- fillets,
- exact measurements,

belong primarily to the CAD geometry kernel.

Do not move an operation to GPU merely because GPU computing sounds faster.

Prioritize correctness and robustness.

---

# 27. Rendering cache requirements

Do not tessellate the entire document every frame.

Maintain revision-aware caches.

Conceptually:

```text
Exact Shape
   ↓
Shape revision
   ↓
Tessellation cache
   ↓
GPU mesh cache

```

When only Body A changes, do not rebuild every other body.

Support background tessellation.

Support multiple quality levels where useful.

Viewport quality and exported CAD precision must remain independent.

---

# 28. Device-loss handling

The graphics device may disappear or reset.

The application must eventually recover from:

- driver restart,
- adapter removal,
- device-loss errors.

A GPU problem should ideally not destroy the open CAD document.

GPU resources must be considered reconstructable caches.

The document itself must not live only in GPU memory.

---

# 29. UI direction

The application should feel familiar to users of Fusion and other modern desktop CAD tools without copying Autodesk intellectual property.

The overall workflow may include:

- top toolbar/ribbon,
- workspace switcher,
- model browser/tree,
- central 3D canvas,
- navigation cube,
- navigation tools,
- contextual property panel,
- timeline,
- status information,
- command search,
- document tabs.

However:

Do NOT copy:

- Autodesk logos,
- Fusion branding,
- Autodesk icons,
- proprietary artwork,
- exact color palettes,
- pixel-identical layout,
- trademarked visual identity.

Create an original professional visual system.

Copy useful interaction principles, not protected assets.

---

# 30. Personalization is part of the product

Provide extensive user settings.

Eventually support customization of:

- Light / Dark / System theme,
- UI density,
- toolbars,
- command groups,
- shortcut keys,
- browser placement,
- timeline visibility and sizing,
- View Cube position,
- selection appearance,
- sketch colors,
- constraint colors,
- grid colors,
- canvas background,
- edge rendering,
- anti-aliasing,
- graphics device,
- tessellation quality,
- display precision,
- default units,
- snapping,
- auto constraints,
- autosave,
- navigation behavior.

Settings should be versioned and migration-capable.

Invalid user settings should not prevent application launch.

---

# 31. Commands must have stable IDs

Do not wire UI buttons directly to anonymous functions scattered throughout the code.

Commands should have a registry.

Conceptually:

```text
CommandDefinition

ID
Name
Category
Default Shortcut
Icon
CanExecute
Execute

```

The same command should be invokable from:

- toolbar,
- menu,
- shortcut,
- command search,
- context menu,

without duplicating implementation.

---

# 32. Performance requirements

Do not prematurely optimize everything.

However, architecture must not make future optimization impossible.

Particularly avoid:

- blocking UI during long imports,
- rebuilding every feature when only one changed,
- tessellating everything after every camera movement,
- duplicating geometry for every component instance,
- storing unnecessary giant managed copies of native geometry,
- recreating GPU buffers every frame.

Long operations should support cancellation where practical.

The user interface must remain responsive.

---

# 33. Assemblies

Component definitions and component instances must remain separate.

Ten instances of the same bolt should not require ten independent exact copies of the bolt geometry.

Use shared definitions plus transforms.

Assemblies should later support:

- rigid joints,
- revolute joints,
- slider joints,
- cylindrical joints,
- planar joints,
- ball joints,
- grounded components,
- limits,
- interference detection.

Do not build advanced assembly functionality before the single-part CAD foundation is stable.

---

# 34. Sheet metal

Sheet metal should remain a dedicated feature family.

Do not fake sheet-metal modelling through ordinary solid modelling alone.

Eventually support:

- thickness rules,
- bend radius,
- K factor,
- bend allowance,
- flange,
- reliefs,
- unfold,
- refold,
- flat patterns,
- DXF export.

However, sheet metal is later than the core part modeller.

---

# 35. Mesh modelling

Mesh data and exact CAD data are different.

Do not treat an STL as though it automatically contains analytic B-Rep geometry.

Mesh tools may eventually include:

- mesh import,
- mesh inspection,
- welding,
- normal repair,
- degenerate triangle removal,
- decimation,
- hole closing,
- remeshing,
- sectioning,
- limited mesh-to-BRep conversion.

Mesh-to-BRep conversions must clearly communicate limitations.

---

# 36. Development order is mandatory

Do not attempt to implement every requested file format or GUI feature immediately.

Use approximately this product progression:

```text
1. Foundation

2. Native CAD kernel bridge

3. Exact primitive geometry

4. Basic viewer

5. Document model

6. Parameters and units

7. Sketch engine

8. Constraint solver

9. Core parametric features

10. Stable topology references

11. Undo / redo / transactions

12. Native save/load

13. Precision and validation

14. STEP import/export

15. Other open exchange formats

16. Manufacturing validation

17. Improved GUI and personalization

18. Assemblies

19. Surface modelling

20. Sheet metal

21. Advanced visualization

22. Commercial/proprietary translators

23. Extended modules

```

A large number of file formats does not take priority over having one cylinder represented, edited, saved and exported correctly.

---

# 37. Milestone-based execution

Do NOT attempt to complete the entire specification in one uncontrolled implementation pass.

Work milestone by milestone.

Every milestone must have:

```text
Objective
Required implementation
Acceptance criteria
Automated tests
Manual validation where appropriate
Known limitations
Regression tests

```

Do not begin the next major milestone until the current milestone is operational enough to support it.

---

# 38. Mandatory development rules

Follow these rules throughout the project:

1. Keep the repository buildable.
2. Prefer small coherent commits.
3. Do not remove tests merely to make CI pass.
4. Do not weaken geometric tolerances simply to force tests to pass.
5. Do not hide native exceptions.
6. Do not silently ignore modelling failures.
7. Do not silently change user geometry.
8. Do not fake successful file imports.
9. Do not advertise unsupported formats.
10. Do not mix GPU rendering structures with the persistent document model.
11. Do not store persistent topology only as edge/face indexes.
12. Do not make meshes the source of truth for exact CAD solids.
13. Do not make proprietary formats dependencies of core modelling.
14. Do not require internet access for normal CAD operation.
15. Do not require Autodesk Fusion to be installed for basic application functionality.
16. Do not make cloud services a hidden dependency.
17. Do not add telemetry that requires sending user CAD files to a server.
18. Preserve offline operation.
19. Treat CAD files as potentially untrusted input.
20. Validate inputs crossing native boundaries.
21. Keep the C/C++ interop API narrow.
22. Release native resources deterministically.
23. Write tests for every important kernel wrapper.
24. Prevent UI thread blocking during expensive operations.
25. Keep model calculations deterministic where practical.

---

# 39. No arbitrary architecture rewrites

Do not replace the selected architecture simply because another library appears easier.

For example, do not decide halfway through development to replace exact B-Rep modelling with a Unity mesh system.

Do not replace Open CASCADE with an unproven hobby geometry library without strong evidence.

Do not replace the dedicated CAD renderer with WPF 3D merely because it takes less code.

Do not eliminate the native geometry layer merely to make the project “100% C#”.

C# is the primary application language.

It is acceptable and expected that mature native libraries handle specialized geometry and translation workloads.

---

# 40. Open-source reference projects

Use the research document's listed open-source projects as engineering references.

Study them to understand:

- architecture,
- API design,
- dependency models,
- CAD workflows,
- constraint solving,
- geometry handling,
- persistence,
- commands,
- selection,
- import/export.

Do NOT blindly copy code.

Respect licenses.

When incorporating code, verify:

- license,
- attribution requirements,
- redistribution obligations,
- static/dynamic linking implications,
- compatibility with the project's intended distribution model.

Maintain a third-party license inventory.

---

# 41. Security

CAD importers process complex binary and textual files.

Treat imported files as untrusted input.

Important protections include:

- size limits where appropriate,
- recursion/depth limits,
- overflow checks,
- memory limits,
- cancellation,
- worker isolation for risky translators,
- safe temporary-file handling,
- extension + file-header validation,
- path traversal prevention in archive formats,
- defensive native interop.

A malformed CAD file must not be allowed to easily compromise the application.

---

# 42. Testing requirements

Testing is not optional.

Create multiple levels of testing.

## Unit tests

Use for:

- units,
- quantities,
- expressions,
- graph logic,
- command state,
- serialization,
- settings.

## Kernel tests

Use for:

- primitives,
- booleans,
- fillets,
- chamfers,
- shell,
- sweep,
- loft,
- validation,
- topology queries.

## Sketch tests

Use for:

- constraint solving,
- DOF,
- conflicts,
- dimensional changes.

## File-format tests

Use known reference files.

Verify:

- import success,
- units,
- body count,
- assembly structure,
- bounding boxes,
- volumes,
- metadata where appropriate.

## Regression tests

Every serious geometry bug should eventually receive a regression test.

## Golden CAD models

Maintain a permanent validation collection.

Examples:

```text
001_cube_10mm.step
002_cylinder.step
003_filleted_block.step
004_hole_pattern.step
005_multi_body.step
006_assembly.step
007_nurbs.step
008_bad_geometry.step
009_large_model.step
010_small_features.step

```

Each should have expected results.

---

# 43. Manufacturing test parts

Create internal reference parts specifically to validate manufacturing workflows.

Examples:

- precision mounting plate,
- bearing block,
- enclosure,
- flange,
- shaft,
- threaded adapter,
- bracket,
- sheet-metal panel later.

Validate exports using independent CAD applications when possible.

Do not assume that because the application's own importer accepts its own export that external software will interpret it identically.

---

# 44. Acceptance criteria are more important than feature count

A feature is not complete merely because:

- it compiles,
- a button exists,
- a method exists,
- one demonstration succeeds.

Example:

A fillet implementation should eventually be tested against:

- convex edges,
- concave edges,
- multiple edges,
- tangent chains,
- very small radii,
- near-maximum radii,
- impossible radii,
- topology changes,
- recomputation,
- undo/redo,
- save/load.

A failed fillet must leave the prior valid body intact.

Apply this philosophy throughout the application.

---

# 45. Agent behavior at the end of each milestone

At the end of each milestone, report:

```text
MILESTONE STATUS

Completed:
...

Automated tests:
Passed: ...
Failed: ...

Manual checks:
...

Known issues:
...

Technical debt introduced:
...

Files/modules changed:
...

Architecture deviations:
...

Recommended next milestone:
...

```

Do not claim completion if important acceptance tests fail.

If a milestone is partially implemented, label it partial.

---

# 46. When research is required

If implementation encounters uncertainty involving:

- Open CASCADE API behavior,
- Direct3D,
- DXGI,
- file specifications,
- numerical geometry,
- licensing,
- commercial SDK requirements,
- Windows behaviour,

consult current authoritative documentation.

Prefer:

1. official documentation,
2. official source repositories,
3. maintained upstream examples,
4. recognized standards,
5. established CAD source code.

Do not invent library APIs.

Do not rely on memory when the current API can be verified.

---

# 47. Original product identity

The program should ultimately become its own CAD application.

Fusion should be considered a benchmark for usability and expected professional workflows.

Do not build a counterfeit Fusion interface.

Develop an original:

- application name,
- logo,
- icon set,
- visual language,
- theme,
- command presentation,
- settings system.

The objective is:

**Fusion-class usability and modelling philosophy**

not:

**an unauthorized Fusion copy.**

---

# 48. Primary definition of success

The project is successful when a user can create a real precision mechanical part using sketches, constraints and parametric features, save it, reopen it, modify it, validate it and export a manufacturing-compatible model that remains geometrically correct in external software.

A simple but extremely reliable model is more valuable than one hundred half-working tools.

The first major engineering objective can be summarized as:

```text
Create
→ Constrain
→ Dimension
→ Model
→ Recompute
→ Inspect
→ Save
→ Reopen
→ Export
→ Re-import
→ Validate

```

That complete chain must work.

---

# 49. Priority hierarchy

When two objectives conflict, use approximately this priority order:

```text
1. User data safety

2. Geometric correctness

3. Numerical robustness

4. Document integrity

5. Stability

6. Parametric reliability

7. Import/export reliability

8. Manufacturing validity

9. UI responsiveness

10. Performance

11. Ease of use

12. Visual polish

13. Advanced features

14. Feature count

```

Never reverse this hierarchy merely to make development appear faster.

---

# 50. Final instruction to the coding agent

You are not being asked to create a visual prototype that resembles CAD software.

You are being asked to incrementally engineer a real CAD system.

Its geometry must be real.

Its parametric relationships must be real.

Its dimensions must be real.

Its imported bodies must be real.

Its exported engineering models must be real.

Its validation must be real.

Its failures must be handled safely.

Its files must preserve user work.

Its rendering must remain separate from its engineering geometry.

Its interface must eventually be professional, customizable and familiar to modern CAD users.

Follow the accompanying deep-research document closely.

Build from the foundation upward.

Do not skip the difficult architectural problems.

Do not hide failures.

Do not sacrifice precision to demonstrate progress.

Do not attempt to implement everything simultaneously.

At every stage, prefer a small amount of correctly engineered, tested functionality over a large amount of unreliable functionality.

The long-term objective is a stable, precise, offline, professional Windows CAD application capable of creating and exchanging models that may be used for real manufacturing.