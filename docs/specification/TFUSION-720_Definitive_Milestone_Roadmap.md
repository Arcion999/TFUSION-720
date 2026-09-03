# TFUSION-720 Definitive Milestone Roadmap

**Repository:** [Arcion999/TFUSION-720](https://github.com/Arcion999/TFUSION-720)  
**Assessment date:** 2026-09-03 (UTC)  
**Assessment mode:** Read-only; no repository files, settings, issues, branches, or commits were changed  
**Authority:** The attached “MASTER IMPLEMENTATION DIRECTIVE FOR THE CAD APPLICATION” and “Dyp forskningsrapport: En presis, offline C#-CAD i Fusion 360-klassen”

## Executive decision

TFUSION-720 is not currently an application or prototype. The GitHub repository is an empty repository shell. There is therefore no existing implementation to retain, refactor, test, or migrate.

The required architecture is technically coherent and is accepted without deviation:

- C#/.NET owns application behavior and the parametric document.
- WPF owns the Windows desktop shell.
- Open CASCADE Technology (OCCT) owns exact B-Rep, analytic, and NURBS geometry.
- A narrow, versioned native C ABI is the only managed/native kernel boundary.
- Direct3D 11 through Vortice.Windows owns the first production viewport.
- Exact geometry is authoritative; tessellated meshes are disposable visualization data.
- Sketch solving, persistence, import/export, transactions, validation, and topology references are independent subsystems.

The first product-level acceptance gate is Milestone 13. At that point this complete chain must work with genuine geometry and persistent design intent:

    Create → Sketch → Constrain → Dimension → Parametric Feature
    → Recompute → Inspect → Save → Close → Reopen → Modify
    → Export STEP → Re-import → Validate

Milestones are hard gates. Milestone 2 must not start until Milestone 1 passes every mandatory criterion.

## Source-control and evidence record

Both attached documents were read completely before repository analysis:

| Document | Lines | Bytes | SHA-256 |
|---|---:|---:|---|
| MASTER IMPLEMENTATION DIRECTIVE FOR THE CAD APPLICATION | 1,706 | 34,608 | 80cd9ca222a5899e2afcc7662c14cf801f69f471924221191af7bb4b022019e3 |
| Deep Research Report | 2,131 | 60,730 | 3c4b8239e38e1903bf17323a4a2242dfb010d4f2f49c9b9b73c75577f11d9168 |

GitHub evidence was collected from the repository metadata, Contents API, refs, commits, rulesets, issues, pull requests, releases, and Actions runs. The Contents API returned “This repository is empty”; commits and refs returned “Git Repository is empty.”

Current external technical baselines were checked against primary sources:

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core): .NET 10 is active LTS through 2028-11-14.
- [.NET 10.0.11 release](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.11/10.0.11.md): current serviced SDK line includes 10.0.400.
- [WPF Desktop SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props-desktop): Windows TFM plus UseWPF=true.
- [.NET global.json behavior](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json): SDK selection and roll-forward behavior.
- [.NET 10 supported operating systems](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md).
- [OCCT 8.0.1 release](https://github.com/Open-Cascade-SAS/OCCT/releases/tag/V8.0.1), published 2026-07-30.
- [Microsoft vcpkg OCCT manifest](https://github.com/microsoft/vcpkg/blob/master/ports/opencascade/vcpkg.json), currently packaging OCCT 8.0.1.
- [Vortice.Windows 3.8.3 package](https://www.nuget.org/packages/Vortice.Direct3D11/3.8.3) and [upstream repository](https://github.com/amerkoleci/Vortice.Windows).
- [actions/checkout v7.0.1](https://github.com/actions/checkout/releases/tag/v7.0.1) and [actions/setup-dotnet v6.0.0](https://github.com/actions/setup-dotnet/releases/tag/v6.0.0).

# Repository Current State

## Repository metadata

| Item | Observed state | Consequence |
|---|---|---|
| Visibility | Public | Suitable for public CI and transparent development |
| Description | “3D modeling Program” | Too broad to serve as a technical project definition |
| Created | 2026-09-03 19:47:54 UTC | Newly created repository |
| Repository size | 0 | No tracked content |
| Default branch field | main | Metadata names main, but no main ref exists |
| Branches / refs | None | No source-control history or protection can yet apply |
| Commits | None | No baseline SHA exists |
| Files / directories | None | No source, docs, configuration, or assets |
| Languages | None detected | No implementation language present |
| License | None | Redistribution and contribution terms are undefined |
| Topics | None | No discoverability metadata |
| Issues | Enabled; zero issues | Tracking is available but unused |
| Pull requests | Enabled; zero PRs | Review flow is available but unused |
| Releases / tags | None | No version or artifact history |
| GitHub Actions runs | Zero | No CI exists |
| Repository rulesets | None | No protected workflow is configured |
| Merge methods | Merge, squash, and rebase all allowed | Policy is not yet standardized |
| Auto-merge | Disabled | Neutral at this stage |
| Delete branch on merge | Disabled | Stale branch cleanup is not automated |

## Files, code, dependencies, build, and tests

There are no files to enumerate. Consequently:

- no C# solution or projects;
- no WPF application;
- no C++ or CMake project;
- no OCCT integration;
- no Vortice.Windows integration;
- no package manifest or lock file;
- no build scripts;
- no CI workflow;
- no tests or test data;
- no coding standards;
- no architecture decision records;
- no security policy;
- no third-party license inventory;
- no native project format;
- no application executable.

The current state is best classified as **pre-bootstrap / 0% implementation**, not as a partially compliant CAD application.

# Specification Compliance

The compliance states below distinguish “not started” from “implemented incorrectly.” Because the repository is empty, almost all requirements are absent rather than contradicted.

| Specification area | State | Evidence / interpretation |
|---|---|---|
| C#/.NET main application | Not started | No projects |
| WPF desktop shell | Not started | No UI code |
| OCCT exact geometry | Not started | No native dependency or code |
| Narrow native C ABI | Not started | No native project |
| Direct3D/Vortice renderer | Not started | No renderer |
| Exact geometry / mesh separation | Not started | No model or mesh types |
| Parametric document/history | Not started | No document model |
| Sketch constraint solver | Not started | No sketch subsystem |
| Modular format providers | Not started | No IO subsystem |
| Versioned native format | Not started | No persistence subsystem |
| Transactional modelling | Not started | No command/transaction system |
| Tolerance taxonomy | Not started | No precision policy |
| Stable topology references | Not started | No topology model |
| Geometry/manufacturing validation | Not started | No validation |
| Atomic saving and recovery | Not started | No persistence |
| Automated test hierarchy | Not started | No tests |
| Offline normal operation | Not contradicted | No cloud dependency exists, but no app exists |
| Proprietary formats isolated from core | Not contradicted | No translator code exists |
| No fake modelling operations | Not contradicted | No UI or modelling code exists |
| Licensing / third-party notices | Failing prerequisite | Repository has no license or notices |
| Protected, continuously buildable main | Failing prerequisite | No main ref, rules, or CI |

No existing code violates the required architecture because there is no existing code. The absence of a project license and a reproducible engineering baseline are immediate blockers for responsible implementation.

# Architecture Assessment

## Required boundary model

    WPF application and commands
               │
               ▼
    Parametric document / sketch / features
               │
               ▼
       Managed kernel interop
               │  versioned C ABI
               ▼
        OCCT exact geometry
               │
               ▼
      Tessellation snapshots ──► Direct3D 11 viewport

Import/export providers attach to the document/kernel boundary. They do not bypass the document integrity rules, and they never convert an exact CAD body into a mesh-only authoritative body.

## Accepted architectural decisions

1. **Runtime and platforms**

   - Use .NET 10 LTS and C# 14.
   - Pin Milestone 1 to SDK 10.0.400 with latest-patch roll-forward inside that feature band.
   - Pure managed domain libraries target net10.0.
   - Windows-specific app, interop, diagnostics, and rendering projects target net10.0-windows.
   - The first supported binary architecture is win-x64.
   - The first formally tested operating-system baseline is Windows 11 24H2 x64 or newer supported Windows 11 servicing releases. Broader compatibility can be added only with an explicit test matrix.

2. **WPF and viewport**

   - WPF owns windows, docking/panels, menus, dialogs, accessibility, and input routing.
   - The production CAD canvas is not WPF 3D.
   - The Direct3D 11 viewport will use a dedicated child HWND hosted by WPF through HwndHost. This avoids the D3D9-based D3DImage bridge and gives the renderer a normal DXGI swap-chain target.
   - The known HWND “airspace” limitation is accepted for the central canvas; WPF overlays must be separate sibling regions or renderer-drawn overlays.

3. **Native kernel**

   - OCCT 8.0.1 is the verified baseline candidate for Milestone 2.
   - Acquire it through vcpkg manifest mode with a pinned vcpkg baseline and explicit x64-windows triplet.
   - Recheck the latest serviced OCCT patch and security state when Milestone 2 starts. A version change requires a small ADR update, not an architecture rewrite.
   - Build a project-owned TFusion.Kernel.Native.dll. C# never binds directly to OCCT C++ classes.

4. **C ABI contract**

   - ABI version starts at 1 and is queryable.
   - Use C-compatible fixed-width types, explicit struct-size/version fields, UTF-8 strings, explicit buffer lengths, and one documented calling convention.
   - Use opaque 64-bit handles backed by a validated registry; never expose C++ pointers as trusted managed values.
   - No C++ exception may cross the ABI.
   - Every call returns a structured status code. Detailed diagnostics are retrieved from the originating kernel context.
   - Managed ownership uses SafeHandle-derived wrappers and deterministic disposal.

5. **Native project format**

   - Use .tf720 as the product-native extension.
   - The file is a versioned ZIP container with a manifest, canonical JSON document data, OCCT B-Rep snapshots, references, and optional preview data.
   - Feature history and sketches remain authoritative; stored B-Rep is a validated acceleration/recovery snapshot, not a replacement for design intent.
   - Saving is temporary-file → flush → container validation → atomic replacement, with backup/recovery behavior.

6. **Error and transaction model**

   - Null is never used to mean “geometry failed.”
   - Managed operations return typed Result values with stable error codes, severity, user-safe text, technical context, and causal diagnostics.
   - Feature calculation occurs against candidate state. Only a valid result commits.
   - A failed feature retains the previous valid body and stores an explicit failed feature state.

7. **Tolerance policy**

   - No global epsilon.
   - PrecisionSettings contains separate modelling, angular, constraint, coincidence, intersection, sewing, import, healing, tessellation, selection, snapping, and export policies.
   - Tolerance escalation is observable, bounded, and included in validation reports.

8. **Topology references**

   - Persistent references combine generating feature identity, kernel history, topology kind, geometric signatures, adjacency, location/direction, and confidence.
   - Array index is allowed only as a transient enumeration detail.
   - Ambiguous remapping fails as Reference Lost; it never silently picks a similar face.

9. **Sketch solver**

   - Implement a dedicated C# subsystem: model → constraint graph → equation generation → numerical solver → residual/DOF analysis → conflict/redundancy diagnostics.
   - FreeCAD PlaneGCS and CADability are research references only. No code is copied without explicit license review and provenance.

10. **Offline and privacy**

    - Opening, editing, saving, validating, importing supported open formats, and exporting work without a network connection.
    - No CAD content telemetry.
    - Diagnostic packages exclude document contents unless the user explicitly opts in.

## Architectural deviation status

**None.** The decisions above refine implementation details while preserving every required responsibility and priority.

If a future incompatibility is found, work stops at the current milestone. An ADR must document:

- the conflicting requirement;
- reproduced evidence;
- alternatives evaluated;
- data-safety, geometry, licensing, and migration impact;
- the smallest proposed deviation;
- new acceptance tests.

No deviation is implemented before that ADR is reviewed.

# Complete Milestone Roadmap

## Universal milestone gate

Every milestone must satisfy all of the following before the next starts:

- repository builds from a clean clone using documented commands;
- required CI checks are green with zero compiler warnings;
- all new automated tests pass and prior regression tests remain green;
- mandatory manual tests are recorded with tester, OS, GPU where relevant, build SHA, and result;
- no skipped or deleted test hides a failure;
- new dependencies are pinned and entered in the third-party inventory;
- no unapproved architecture deviation exists;
- failure paths are tested, not only success paths;
- milestone report declares completed work, tests, known issues, debt, changed modules, and deviations;
- any mandatory criterion not met makes the milestone **FAIL or PARTIAL**, never PASS.

## Milestone matrix

### Milestone 1 — Repository and engineering foundation

**Objective:** Establish a deterministic, warning-free, testable .NET/WPF repository with enforced boundaries, real diagnostics, governance, and no pretend CAD functionality.

**Automated gate:** Clean locked restore; Release x64 build; unit and architecture tests; formatting; dependency audit; deterministic managed-output check.

**Manual gate:** Clean Windows clone builds; minimal WPF shell launches and exits cleanly; diagnostics self-test works offline; forced termination is detected on next launch.

**PASS:** Every detailed Milestone 1 criterion in the later section passes.

**FAIL:** Missing license decision, non-reproducible restore, warnings, red tests, missing authoritative documents, fake modelling UI, unpinned dependencies, or absent CI.

### Milestone 2 — OCCT native bridge foundation

**Objective:** Build and load TFusion.Kernel.Native.dll backed by real OCCT, with a stable C ABI, context/error handling, opaque handles, and deterministic ownership.

**Automated gate:** ABI/version negotiation; create/destroy contexts; invalid/stale/double-release handle tests; exception containment; UTF-8 diagnostics; 10,000 lifecycle iterations under leak/sanitizer tooling; x64 packaging/load test.

**Manual gate:** Diagnostics executable reports the compiled and runtime OCCT versions from a clean Windows machine without PATH modifications.

**PASS:** Real OCCT code executes across the ABI; no direct OCCT type leaks into C#; invalid inputs return structured errors; leak checks pass.

**FAIL:** Stubbed success, raw pointer exposure, C++ exception crossing ABI, manual DLL copying outside the build, nondeterministic ownership, or crash on invalid handle.

**Do not start:** Geometry features, renderer, or file import.

### Milestone 3 — Exact primitive geometry and kernel validation

**Objective:** Represent and validate exact points, axes, planes, transforms, curves, topology, and primitive solids in OCCT.

**Automated gate:** Analytic box/cylinder/cone/sphere/torus volume and bounds; transform composition; topology counts/types; BRepCheck validation; invalid dimensions; extreme-but-supported scales; clone/release stress; failure rollback.

**Manual gate:** Diagnostics prints exact properties and validation reports for reference primitives.

**PASS:** Primitives are real OCCT B-Reps with correct analytic surfaces within declared modelling tolerances.

**FAIL:** Mesh-only primitives, hidden tolerance inflation, unchecked invalid shapes, null-as-failure, or wrong analytic measurements.

### Milestone 4 — D3D11 viewport and disposable tessellation

**Objective:** Display OCCT shapes through tessellation snapshots and a separate Direct3D 11 renderer using Vortice.Windows.

**Automated gate:** WARP render smoke tests; tessellation revision/cache tests; device/resource disposal; resize/DPI tests; deterministic picking IDs; unchanged-body cache retention; renderer has no document mutation API.

**Manual gate:** Hardware tests on integrated GPU, NVIDIA/AMD discrete GPU where available, and WARP; orbit/pan/zoom/fit; resize; multi-monitor DPI; simulated device loss.

**PASS:** Exact shape remains authoritative, meshes regenerate, the UI stays responsive, and hardware absence falls back safely.

**FAIL:** WPF 3D as production renderer, geometry stored only on GPU, full retessellation every frame, NVIDIA requirement, or device loss destroys document state.

### Milestone 5 — Parametric document, dependency graph, and transactions

**Objective:** Implement CadDocument, components/bodies, feature contracts, dependency graph, dirty propagation, transactional candidate state, and partial recompute.

**Automated gate:** DAG ordering; cycle rejection; deterministic recompute; dirty-subgraph propagation; unrelated-feature non-recompute; failed candidate rollback; component definition versus instance identity; concurrent cancellation.

**Manual gate:** Diagnostics creates a document graph, changes one input, and displays only the expected recomputed nodes.

**PASS:** Failed calculations preserve last valid state and document invariants.

**FAIL:** Whole-document mutation before validation, cycle acceptance, feature success without a valid body, or full recompute for every edit.

### Milestone 6 — Units, quantities, parameters, and expressions

**Objective:** Add dimension-aware quantities, unit conversion, named parameters, expression parsing, dependency evaluation, and cycle/error reporting.

**Automated gate:** SI/imperial conversions; angle handling; dimensional algebra; locale-independent serialization; parser precedence; supported functions; cycle detection; invalid-unit rejection; display precision has no geometry effect.

**Manual gate:** Edit a parameter expression in both decimal-comma and decimal-point OS locales and verify invariant stored data.

**PASS:** Incompatible dimensions are rejected and recalculation is deterministic.

**FAIL:** Bare doubles as the persistent engineering-value model, locale-corrupted files, silent unit coercion, or display rounding changes model values.

### Milestone 7 — Sketch model and profile construction

**Objective:** Implement sketch planes, stable entity IDs, point/line/circle/arc/ellipse primitives, construction geometry, projection references, and closed-profile extraction.

**Automated gate:** Entity geometry; plane transforms; closed/open loop recognition; nesting and holes; self-intersection reporting; projection invalidation; scale/pathology corpus.

**Manual gate:** Create and edit simple profiles with only commands whose geometry is implemented.

**PASS:** Profiles are mathematically valid inputs to later exact features.

**FAIL:** Visual-only sketch strokes, implicit mesh geometry, ambiguous profile selection, or a “fully constrained” indicator before the solver exists.

### Milestone 8 — Dedicated sketch constraint solver

**Objective:** Solve coincident, horizontal, vertical, distances, radius/diameter, parallel, perpendicular, tangent, equal, concentric, midpoint, symmetry, and fix/unfix with diagnostics.

**Automated gate:** Known-solution corpus; residual thresholds; DOF counts; under/fully/over-constrained states; redundancy and conflict identification; bad initial guesses; near-singular geometry; cancellation; deterministic repeated solve.

**Manual gate:** Fully constrain representative plate, slot, bolt-circle, tangent-arc, and symmetric profiles; deliberately create and resolve a conflict.

**PASS:** Reported status is derived from solver state, residuals are within the sketch policy, and failures do not crash the UI.

**FAIL:** Ad-hoc coordinate snapping presented as solving, tolerance weakening, false fully-constrained state, or no conflict information.

### Milestone 9 — First parametric solid feature chain

**Objective:** Convert solved sketch profiles into exact extrude and revolve features, with new/join/cut/intersect operations and a basic hole feature.

**Automated gate:** Analytic dimensions/volume; open-profile rejection; multi-loop profiles; positive/negative/symmetric extents; Boolean result validity; parameter-driven recompute; cancellation; failure rollback; undo transaction hooks.

**Manual gate:** Build a dimensioned mounting plate, modify sketch and thickness parameters, and inspect recomputed exact values.

**PASS:** The mounting plate remains a valid exact B-Rep through edits.

**FAIL:** Mesh-based operations, stale display versus exact body, corrupt previous body on Boolean failure, or hard-coded demo geometry.

### Milestone 10 — Stable topology references and core modifiers

**Objective:** Implement topology history/signature resolution and then fillet, chamfer, shell, draft, mirror, rectangular/circular pattern, split, and combine.

**Automated gate:** Edge/face split and reorder corpus; generated/modified history; signature fallback confidence; ambiguous-reference failure; convex/concave/impossible fillets; shell limits; pattern changes; recompute/rollback regression.

**Manual gate:** Modify upstream plate dimensions so edge ordering changes; dependent features either remain correctly attached or report Reference Lost.

**PASS:** No feature relies solely on topology array indexes and ambiguity is never silently resolved.

**FAIL:** Wrong-face attachment, silent remap, geometry corruption, or modifier shown without true B-Rep change.

### Milestone 11 — Command history, native save/load, and recovery

**Objective:** Implement stable command IDs, undo/redo transactions, versioned .tf720 persistence, migrations, atomic save, journal, autosave snapshot, and crash recovery.

**Automated gate:** Command execute/undo/redo; failed-command exclusion; deterministic canonical JSON; schema round-trip; unknown-field/version behavior; migration fixtures; interrupted save; disk-full simulation; corrupt ZIP/path traversal rejection; journal replay; stale B-Rep snapshot regeneration.

**Manual gate:** Save, close, reopen, compare full design intent; kill process after edits and recover to a separate document; verify original saved file remains intact.

**PASS:** User work survives normal and tested abnormal flows without overwriting the only valid copy.

**FAIL:** STEP used as native format, non-atomic overwrite, feature history loss, unbounded archive extraction, or corrupt file accepted as valid.

### Milestone 12 — Inspection, precision, and manufacturing health

**Objective:** Implement exact measurement and model-health checks: validity, closed solids, manifold topology, orientation, open boundaries, self-intersection diagnostics, degenerate/tiny entities, units, and tolerance reports.

**Automated gate:** Analytic measurements; invalid and non-manifold corpus; imported-tolerance outliers; issue localization; conservative healing audit; readiness decision rules.

**Manual gate:** Inspect valid and deliberately damaged reference parts; verify every PASS/FAIL reason links to affected geometry.

**PASS:** “Manufacturing Ready” is a deterministic result of real checks.

**FAIL:** Cosmetic status, hidden healing, missing tolerance provenance, or readiness PASS for invalid/open geometry.

### Milestone 13 — STEP AP242 and end-to-end CAD foundation gate

**Objective:** Implement modular format-provider contracts and OCCT/XDE STEP AP242 import/export, followed by fresh-context round-trip validation.

**Automated gate:** Golden STEP corpus; units/names/colors/assemblies where supported; body/component counts; exact validity; fresh kernel-context re-import; volume/area/bounds/critical-dimension comparison; corrupt/cancelled import; export failure leaves no valid-looking partial file.

**Manual gate:** Complete the full Create-to-Validate chain and open exported STEP in at least two independent CAD/CAM tools.

**PASS:** The full chain at the start of this report works reliably, external tools accept the STEP file, and deviations stay inside declared tolerances.

**FAIL:** Export without fresh re-import, self-import only as evidence, mesh in place of B-Rep, lost units, unexplained healing, or any broken link in the chain.

### Milestone 14 — Open and neutral exchange providers

**Objective:** Add providers in controlled order: IGES; STL/OBJ mesh; 3MF via lib3mf; SVG/DXF sketch; 3DM via openNURBS.

**Automated gate:** One golden corpus per format; header plus extension detection; unit/bounds/count checks; hostile/corrupt inputs; cancellation; mesh/B-Rep separation; provider capability reporting.

**Manual gate:** Cross-open outputs in independent applications and record limitations.

**PASS:** Each advertised capability is demonstrated by fixtures and an honest capability matrix.

**FAIL:** One giant importer, fake success, STL treated as analytic B-Rep, unsupported extension advertised, or silent data loss.

### Milestone 15 — Professional interaction and personalization

**Objective:** Complete honest UI for existing functions: browser, timeline, properties, selection/preselection, navigation, command registry/search, shortcuts, themes, units, precision, and graphics settings.

**Automated gate:** Command-ID uniqueness; CanExecute state; shortcut conflicts; settings schema/migration/fallback; selection filter tests; accessibility automation properties; invalid settings recovery.

**Manual gate:** Keyboard-only basic workflow; high DPI; light/dark/high contrast; navigation profiles; no functional control without a real command.

**PASS:** UI exposes only implemented behavior and settings failure cannot block startup.

**FAIL:** Fake buttons, duplicated anonymous command logic, Autodesk asset copying, inaccessible core workflow, or settings corruption prevents launch.

### Milestone 16 — Responsiveness, device loss, and large-model stability

**Objective:** Add cancellation, background recompute/tessellation, revision-aware caches, GPU recovery, memory budgets, instrumentation, and large-model modes.

**Automated gate:** Cancellation races; stale-result rejection; device-loss recreation; memory/handle soak; thousands-of-feature document; instancing; cache invalidation; UI-thread blocking thresholds.

**Manual gate:** Long import/recompute remains cancellable; GPU driver reset simulation; integrated/discrete switching; multi-hour soak.

**PASS:** Failures degrade capability without losing the document.

**FAIL:** stale asynchronous commit, UI hangs without cancellation, device loss terminates editing state, or unbounded memory growth.

### Milestone 17 — Assemblies

**Objective:** Add shared component definitions, instances/transforms, grounding, rigid/revolute/slider/cylindrical/planar/ball joints, limits, and interference.

**Automated gate:** Nested transforms; instance sharing; joint kinematics/limits; cycle rejection; interference truth set; save/load and STEP assembly preservation.

**Manual gate:** Build and manipulate a small mechanism with repeated fasteners without duplicated exact geometry.

**PASS:** Instances share definitions and joints solve consistently.

**FAIL:** copied body per instance, transform drift, invalid joint state committed, or unreliable interference claims.

### Milestone 18 — Advanced surface and NURBS modelling

**Objective:** Add surface extrude/revolve/sweep/loft, patch/fill, offset, extend, trim, split, sew, unstitch, thicken, ruled surfaces, and continuity analysis.

**Automated gate:** NURBS evaluation; trim boundaries; G0/G1/G2 checks where supported; sewing tolerance audit; self-intersection; thicken failure rollback; surface regression corpus.

**Manual gate:** Build representative lofted housing and Class-A-like transition test parts; inspect continuity and export STEP.

**PASS:** Exact surfaces remain exact and all healing/tolerance changes are reported.

**FAIL:** triangulated substitute, hidden gap closure, false continuity claim, or invalid shell commit.

### Milestone 19 — Sheet metal

**Objective:** Add dedicated rules, constant thickness, bend radius, K-factor/allowance, flange, relief, unfold/refold, flat pattern, and DXF.

**Automated gate:** neutral-axis/bend calculations; thickness validation; bend/relief corpus; unfold-refold comparison; flat-pattern dimensions; invalid-rule rejection.

**Manual gate:** Manufacture-oriented bracket/panel parts checked against independent sheet-metal software.

**PASS:** Folded and flat states are associatively linked and dimensionally verified.

**FAIL:** ordinary solid approximation advertised as sheet metal, thickness inconsistency ignored, or unverified flat pattern.

### Milestone 20 — Translator isolation and optional proprietary formats

**Objective:** Implement CadTranslatorWorker.exe sandboxing/IPC and only then integrate licensed translators such as HOOPS or ODA behind capability flags.

**Automated gate:** worker crash/timeout/memory cap; malformed input; IPC versioning; temp-path safety; provider absence; licensed fixture matrices; semantic preservation and audit reports.

**Manual gate:** Test supported vendor/version matrix using legally obtained fixtures and verify license/redistribution terms.

**PASS:** Core CAD remains fully functional offline without commercial SDKs; only tested versions are advertised.

**FAIL:** proprietary SDK becomes a core dependency, reverse engineering to claim support, worker crash kills editor, or unsupported version silently imports partial geometry.

### Milestone 21 — Production release hardening

**Objective:** Produce a signed x64 Windows release with controlled DLL loading, safe mode, startup-crash recovery, installer/update rollback strategy, privacy-safe diagnostics, and release qualification.

**Automated gate:** clean-VM install/uninstall; signature verification; DLL search-path tests; dependency/license manifest; safe-mode boot; update/rollback; full regression, fuzz, soak, and security scan.

**Manual gate:** release candidate workflow on representative integrated, AMD, and NVIDIA systems; offline operation; external manufacturing exchange qualification.

**PASS:** Signed release passes the complete qualification matrix with no open severity-1 data-loss or geometry-correctness defect.

**FAIL:** unsigned/incorrect binaries, unsafe DLL loading, cloud requirement, unrecoverable installer failure, or open critical correctness/data-loss issue.

### Milestone 22 — Explicitly deferred product areas

**Objective:** Evaluate drawings, richer mesh/SubD/Form, CAM, FEA/simulation, PCB/electronics, scripting, and collaboration as independent modules only after the CAD foundation is production-stable.

**Automated/manual gate:** Each proposed module receives its own specification, architecture boundary, threat/license analysis, and acceptance corpus before implementation.

**PASS:** A module starts only after approval and without weakening the core.

**FAIL:** advanced-module work diverts resources from an unpassed core milestone or introduces mandatory cloud coupling.

# Milestone 1 Detailed Implementation Specification

## Exact objective

Create the first real, reviewable repository baseline. Milestone 1 proves that TFUSION-720 can be built, tested, diagnosed, and evolved safely. It does not implement CAD geometry.

The output must be a minimal WPF executable and diagnostics executable backed by useful foundation code, not a screen mockup. The WPF shell must contain no modelling toolbar, model tree, timeline, viewport, import dialog, or command that suggests CAD functionality exists.

## Bootstrap procedure

The repository has no commit and therefore no actual main branch. The unavoidable bootstrap exception is:

1. Create one initial main commit containing only README.md, the selected LICENSE, .gitignore, and the two authoritative specification documents plus their hash manifest.
2. Immediately create branch milestone/01-foundation.
3. Implement the rest of Milestone 1 as small coherent commits on that branch.
4. Create a pull request to main.
5. Configure a repository ruleset after main exists: require the Milestone 1 CI checks, block force-push and deletion, require PR-based changes, and retain an owner emergency bypass.
6. Merge only when the detailed PASS gate is met.

The initial main commit is a repository-creation constraint, not an architecture deviation.

## Required repository structure

    /
    ├── .config/
    │   └── dotnet-tools.json
    ├── .github/
    │   ├── CODEOWNERS
    │   ├── dependabot.yml
    │   ├── pull_request_template.md
    │   └── workflows/
    │       ├── ci.yml
    │       └── codeql.yml
    ├── docs/
    │   ├── architecture/
    │   │   ├── ADR-0001-system-boundaries.md
    │   │   ├── ADR-0002-native-c-abi.md
    │   │   ├── ADR-0003-exact-geometry-and-render-mesh.md
    │   │   ├── ADR-0004-results-errors-and-transactions.md
    │   │   ├── ADR-0005-tolerance-taxonomy.md
    │   │   ├── ADR-0006-platform-toolchain-and-viewport.md
    │   │   └── README.md
    │   ├── milestones/
    │   │   ├── M01-foundation.md
    │   │   └── README.md
    │   └── specification/
    │       ├── MASTER_IMPLEMENTATION_DIRECTIVE.md
    │       ├── DEEP_RESEARCH_REPORT.md
    │       └── SOURCES.sha256
    ├── eng/
    │   ├── build.ps1
    │   ├── clean.ps1
    │   ├── test.ps1
    │   └── verify.ps1
    ├── src/
    │   ├── TFusion.App/
    │   │   ├── App.xaml
    │   │   ├── App.xaml.cs
    │   │   ├── MainWindow.xaml
    │   │   ├── MainWindow.xaml.cs
    │   │   ├── CompositionRoot.cs
    │   │   └── TFusion.App.csproj
    │   ├── TFusion.Diagnostics/
    │   │   ├── Program.cs
    │   │   ├── SelfTestCommand.cs
    │   │   └── TFusion.Diagnostics.csproj
    │   └── TFusion.Foundation/
    │       ├── Diagnostics/
    │       │   ├── CadDiagnostic.cs
    │       │   ├── DiagnosticCode.cs
    │       │   └── DiagnosticSeverity.cs
    │       ├── Identifiers/
    │       │   ├── EntityIds.cs
    │       │   └── StrongGuid.cs
    │       ├── Lifecycle/
    │       │   └── StartupSentinel.cs
    │       ├── Results/
    │       │   ├── Result.cs
    │       │   └── ResultOfT.cs
    │       ├── Storage/
    │       │   └── ProductPaths.cs
    │       ├── BuildInfo.cs
    │       └── TFusion.Foundation.csproj
    ├── tests/
    │   ├── TFusion.Architecture.Tests/
    │   │   ├── ProjectBoundaryTests.cs
    │   │   ├── RepositoryPolicyTests.cs
    │   │   └── TFusion.Architecture.Tests.csproj
    │   └── TFusion.Foundation.Tests/
    │       ├── DiagnosticsTests.cs
    │       ├── ProductPathsTests.cs
    │       ├── ResultTests.cs
    │       ├── StartupSentinelTests.cs
    │       ├── StrongGuidTests.cs
    │       └── TFusion.Foundation.Tests.csproj
    ├── .editorconfig
    ├── .gitattributes
    ├── .gitignore
    ├── CONTRIBUTING.md
    ├── Directory.Build.props
    ├── Directory.Build.targets
    ├── Directory.Packages.props
    ├── global.json
    ├── LICENSE
    ├── NuGet.config
    ├── README.md
    ├── SECURITY.md
    ├── THIRD_PARTY_NOTICES.md
    └── TFUSION-720.sln

Do not create empty future projects such as Sketch, Rendering, Kernel.Interop, Persistence, or IO in Milestone 1. They are added when they first contain tested behavior. This preserves the required modular architecture without accumulating placeholder assemblies.

## Project responsibilities and references

| Project | Target | Responsibility | Allowed TFusion references |
|---|---|---|---|
| TFusion.Foundation | net10.0 | Diagnostics, typed results, stable ID primitives, product paths, startup sentinel, build info | None |
| TFusion.App | net10.0-windows; WPF; WinExe; x64 | Process composition root, local logging, lifecycle, honest minimal window | Foundation |
| TFusion.Diagnostics | net10.0-windows; Exe; x64 | Machine-readable self-test and environment/build diagnostics | Foundation |
| TFusion.Foundation.Tests | net10.0 | Foundation behavior | Foundation |
| TFusion.Architecture.Tests | net10.0 | Repository and dependency-boundary enforcement | Foundation only if needed |

TFusion.Foundation must not reference WPF, WindowsBase, PresentationCore, PresentationFramework, OCCT, Vortice, P/Invoke, import/export SDKs, or networking packages.

## Toolchain and dependency baseline

### Toolchain

- .NET SDK 10.0.400 in global.json.
- rollForward: latestPatch.
- allowPrerelease: false.
- C# language version 14.0.
- Windows build architecture x64.
- PowerShell 7 for engineering scripts.
- Visual Studio 2026 with .NET desktop development and Desktop development with C++ workloads is the documented developer environment; C++ is not used until Milestone 2.

### NuGet dependencies

Use central package management and exact versions:

| Package | Version | Scope |
|---|---:|---|
| Microsoft.Extensions.Hosting | 10.0.11 | App composition/lifecycle |
| Serilog | 4.4.0 | Structured event model |
| Serilog.Extensions.Hosting | 10.0.0 | Host integration |
| Serilog.Sinks.File | 7.0.0 | Local rolling structured logs |
| xunit.v3 | 4.0.0 | Test framework |
| Microsoft.NET.Test.Sdk | 18.9.0 | Test discovery/execution |
| coverlet.collector | 10.0.1 | Coverage collection |

Before implementation, the coding agent must verify these exact packages still resolve from nuget.org and record package license metadata in THIRD_PARTY_NOTICES.md. Do not silently substitute prerelease packages.

Do not add OCCT, vcpkg, Vortice.Windows, CommunityToolkit, docking frameworks, JSON alternatives, dependency-injection containers, format SDKs, cloud SDKs, telemetry packages, or UI component suites in Milestone 1.

### Build policy

Directory.Build.props must establish:

- Nullable=enable;
- ImplicitUsings=enable;
- LangVersion=14.0;
- TreatWarningsAsErrors=true;
- EnforceCodeStyleInBuild=true;
- AnalysisLevel=10.0-recommended;
- Deterministic=true;
- ContinuousIntegrationBuild=true when CI is set;
- PlatformTarget=x64 for executable Windows projects;
- no unsafe blocks unless a later project explicitly documents and tests the need.

Directory.Packages.props must enable central package management and transitive pinning. Commit packages.lock.json for every package-consuming project. CI restores in locked mode.

## Required implementation behavior

### Result and diagnostic contracts

Result and Result<T> are immutable and enforce these invariants:

- success contains no error diagnostics;
- failure contains at least one error diagnostic;
- a successful Result<T> contains a value;
- callers cannot access a failed result value without an explicit exception that indicates programmer misuse;
- diagnostic code is stable and machine-readable;
- user-facing message and technical context are separate;
- an exception may be retained for local logging but must not be required for serialization.

CadDiagnostic contains:

- DiagnosticCode;
- DiagnosticSeverity;
- user-safe message;
- optional technical detail;
- optional immutable key/value context;
- optional causal diagnostic chain.

Initial codes cover foundation/configuration/storage/startup failures. Do not define fake geometry errors before kernel work begins.

### Stable identifiers

StrongGuid is an immutable value type that:

- rejects Guid.Empty;
- creates IDs through a cryptographically suitable framework GUID generator;
- parses and formats with invariant “D” representation;
- compares and hashes by value;
- serializes without culture dependence.

EntityIds.cs defines distinct wrappers for DocumentId, ComponentId, BodyId, FeatureId, SketchId, SketchEntityId, ConstraintId, ParameterId, and CommandId. Each wrapper must be type-safe; a FeatureId cannot be passed where a BodyId is required.

### Product paths

ProductPaths resolves local state below one application root, normally:

    %LOCALAPPDATA%\TFUSION-720\
      Logs\
      Recovery\
      CrashDumps\
      Settings\
      Temp\

The root must be injectable for tests. Directory creation returns Result, validates paths, and never falls back to the current working directory. Log retention is bounded. Logs must not contain CAD content or full model paths by default.

### Startup sentinel

StartupSentinel writes a small versioned marker atomically:

- process start marks the session unclean;
- orderly shutdown marks it clean;
- a pre-existing unclean marker is reported on next start;
- corrupt marker content produces a diagnostic and safe reset;
- marker writes cannot overwrite user CAD files because they are restricted to ProductPaths.

This is crash-detection infrastructure, not document recovery. Recovery journaling is Milestone 11.

### Structured logging

Configure Serilog in CompositionRoot:

- compact JSON lines in the Logs directory;
- daily rolling files;
- explicit retained-file count and maximum file size;
- UTC timestamps;
- application version, process ID, thread ID where available, and session ID;
- startup, shutdown, unhandled managed exception, dispatcher exception, and unobserved task exception events;
- no network sink;
- no automatic upload;
- no document contents.

Exception hooks log and fail safely. They must not label an unknown corrupted state as recoverable; unrecoverable dispatcher exceptions lead to a controlled shutdown after logging.

### Diagnostics executable

TFusion.Diagnostics supports:

    TFusion.Diagnostics.exe --self-test --format json

It returns:

- exit 0 only if the runtime is supported, architecture is x64, application directories can be safely created, startup-sentinel round-trip succeeds in an isolated test location, and build metadata is readable;
- nonzero exit for failure;
- one valid JSON object on stdout;
- technical details on stderr/logs without secrets.

It does not claim the CAD kernel, renderer, or format providers are available in Milestone 1.

### WPF application

The application:

- starts through a Generic Host composition root;
- initializes logging before the window;
- uses AppData paths rather than the repository or executable directory;
- creates the startup sentinel;
- opens one minimal original TFUSION-720 window;
- displays application/version and an explicit “Engineering foundation — CAD tools not implemented yet” status;
- contains no CAD buttons, ribbon, model browser, fake canvas, fake import, fake feature tree, or hard-coded sample geometry;
- closes the host, flushes logs, and marks a clean shutdown.

The window is a smoke-test surface only, not a UI prototype.

## Engineering scripts

- eng/build.ps1: locked restore followed by Release x64 build; exits nonzero on any failure.
- eng/test.ps1: runs all tests, writes TRX and coverage beneath artifacts/test-results, and propagates failures.
- eng/verify.ps1: verifies formatting, locked restore, Release build, tests, dependency vulnerability audit, solution/project policy, and deterministic Foundation DLL output.
- eng/clean.ps1: removes only known repository-local bin, obj, TestResults, and artifacts paths. It validates the repository root and must never recursively target an unresolved or broad path.

Scripts are non-interactive and use strict error handling. They do not modify developer-global configuration.

## CI and repository policy

ci.yml contains at least:

1. **managed-core job on Ubuntu**
   - checkout;
   - install .NET SDK 10.0.400;
   - locked restore;
   - build and test Foundation.

2. **windows-x64 job on windows-2025**
   - checkout;
   - install .NET SDK 10.0.400;
   - execute eng/verify.ps1;
   - retain test results and logs when the job fails.

Use immutable action commit pins with tag comments:

- actions/checkout v7.0.1 → 3d3c42e5aac5ba805825da76410c181273ba90b1
- actions/setup-dotnet v6.0.0 → a98b56852c35b8e3190ac28c8c2271da59106c68

CodeQL scans C# on pull requests and a schedule. Dependabot covers NuGet and GitHub Actions, but updates must pass the same tests and must not auto-merge a major version.

After the first main ref exists, configure required checks:

- managed-core;
- windows-x64;
- CodeQL where GitHub exposes it as a stable required check.

Block force-push and branch deletion. Require PRs. Resolve review conversations. Keep an explicit emergency owner bypass because this is currently a single-owner repository.

## Documentation requirements

README.md states:

- the application is pre-alpha and contains no CAD functionality after Milestone 1;
- the authoritative architecture and priority order;
- supported development OS/toolchain;
- exact build/test commands;
- the current milestone and status;
- no format-support claims;
- offline-operation goal;
- links to specifications, ADRs, security policy, and milestone report.

The two authoritative documents are committed verbatim under docs/specification. SOURCES.sha256 contains their hashes from this report. Any future change to them must be a visible specification revision, never an unnoticed cleanup.

Each ADR contains status, context, decision, consequences, rejected alternatives, and links to relevant specification sections.

M01-foundation.md uses the milestone status template required by the Master Directive and contains the signed-off PASS/FAIL checklist.

## Prohibited shortcuts

Milestone 1 must not:

- add a decorative CAD interface;
- add disabled or TODO modelling buttons;
- draw a cube, grid, or viewport and imply modelling exists;
- create empty future assemblies solely to resemble the final tree;
- add OCCT or Vortice before their milestone;
- use WPF 3D;
- include cloud, account, collaboration, or telemetry code;
- use null as an error result;
- catch and ignore exceptions;
- allow compiler warnings;
- use floating NuGet versions;
- omit package lock files;
- copy code from FreeCAD, CADability, or any reference project;
- choose a project license on the owner’s behalf;
- claim Windows or GPU support that has not been tested;
- begin sketch, geometry, persistence, format, or rendering implementation.

## Automated tests

### Foundation unit tests

| ID | Test |
|---|---|
| M1-U01 | Result success/failure invariants cannot be constructed inconsistently |
| M1-U02 | Failed Result<T> cannot expose a value |
| M1-U03 | Diagnostics preserve code/severity/context and immutable causal order |
| M1-U04 | Every strong ID rejects empty, round-trips invariant text, compares by value, and cannot cross-assign at compile time |
| M1-U05 | ProductPaths creates only descendants of injected root |
| M1-U06 | Invalid/unwritable ProductPaths returns failure and never falls back to working directory |
| M1-U07 | StartupSentinel clean-start/clean-stop round-trip |
| M1-U08 | Existing unclean sentinel is detected |
| M1-U09 | Corrupt sentinel returns a diagnostic and resets safely |
| M1-U10 | Atomic marker replacement never exposes partial JSON in the injected filesystem fault tests |
| M1-U11 | BuildInfo is present and parseable |
| M1-U12 | Logging configuration uses bounded local file retention and no network sink |

### Architecture and repository tests

| ID | Test |
|---|---|
| M1-A01 | Solution contains exactly the five Milestone 1 projects |
| M1-A02 | Foundation references no other TFusion assembly |
| M1-A03 | Foundation has no WPF/WinForms/DirectX/native interop/network package reference |
| M1-A04 | App and Diagnostics reference Foundation; no reverse reference exists |
| M1-A05 | No DllImport/LibraryImport exists in Milestone 1 |
| M1-A06 | No project contains unsafe code |
| M1-A07 | All package versions are central, exact, locked, and non-prerelease |
| M1-A08 | Authoritative specification files match recorded SHA-256 hashes |
| M1-A09 | No tracked bin, obj, artifacts, IDE-user file, dump, or log |
| M1-A10 | Every workflow action is pinned to a full commit SHA |
| M1-A11 | README contains no supported modelling or format claim |
| M1-A12 | THIRD_PARTY_NOTICES covers every direct/transitive distributed dependency |

### Build and process tests

| ID | Test |
|---|---|
| M1-B01 | Clean locked restore succeeds |
| M1-B02 | Release x64 solution build succeeds with zero warnings |
| M1-B03 | All test assemblies pass |
| M1-B04 | dotnet format verification passes |
| M1-B05 | No unsuppressed known high/critical dependency vulnerability |
| M1-B06 | Two clean builds produce byte-identical TFusion.Foundation.dll |
| M1-B07 | Diagnostics self-test produces valid JSON and exit 0 offline |
| M1-B08 | Diagnostics invalid-argument path returns nonzero and machine-readable error |

Coverage is a guardrail, not proof. Require 90% line and 85% branch coverage for TFusion.Foundation in Milestone 1, with no exclusion of result, path, or lifecycle code. Architecture tests and trivial generated WPF code are not included in this threshold.

## Manual tests

Run on a clean Windows 11 24H2-or-newer x64 VM:

| ID | Procedure | Expected result |
|---|---|---|
| M1-M01 | Clone and execute eng/verify.ps1 | One-command verification passes |
| M1-M02 | Start Release TFusion.App | Original minimal window opens; version/status are truthful; no CAD controls |
| M1-M03 | Close normally and restart | Both launches succeed; prior session is clean |
| M1-M04 | Start, force-kill process, restart | Unclean prior launch is logged/detected; app still starts |
| M1-M05 | Disconnect network and start app plus diagnostics | Both function normally; no connection attempt is required |
| M1-M06 | Use a standard user account | Logs/state are written only in user-local application storage |
| M1-M07 | Make application storage unwritable in a disposable test profile | Clear controlled failure; no writes to executable/current directory |
| M1-M08 | Inspect logs | Structured, bounded, no CAD content, secrets, or unexpected full paths |

Record OS build, .NET SDK/runtime, git SHA, account type, commands, and result.

## PASS criteria

Milestone 1 is PASS only when:

1. Every required file/project exists and has the stated responsibility.
2. The owner has selected and committed a project license.
3. Both specification documents match the recorded hashes.
4. All M1-U, M1-A, M1-B, and M1-M tests pass.
5. CI is green on the pull-request head and merged main.
6. Build produces zero warnings.
7. Foundation coverage meets both thresholds.
8. Locked restore and deterministic managed build checks pass.
9. No high/critical unsuppressed dependency vulnerability exists.
10. Third-party inventory is complete for distributed dependencies.
11. Main rules prevent force-push/deletion and require the designated checks.
12. The WPF UI contains no false functionality.
13. Milestone report contains no unapproved deviation and honestly lists limitations.

## FAIL criteria

Milestone 1 is FAIL if any mandatory PASS item is missing, or if:

- a test is skipped, weakened, or removed to gain a green result;
- warnings are suppressed without a narrow documented reason;
- restore depends on an unpinned/floating package;
- the app requires admin rights or network access to launch;
- an unhandled expected filesystem failure crashes the process;
- source specifications were summarized instead of committed verbatim;
- the repository claims geometry, rendering, import/export, or file support;
- future placeholder projects/classes/buttons are added;
- user/model data could be written outside the controlled product root by fallback;
- a direct dependency lacks provenance/license review.

## Expected deliverables

- Initial bootstrap commit and Milestone 1 pull request.
- Buildable TFUSION-720.sln.
- Minimal truthful TFusion.App.exe.
- Working TFusion.Diagnostics.exe self-test.
- Tested Foundation library.
- CI and CodeQL workflows.
- Locked and centrally managed dependencies.
- Complete governance/security/license files.
- Six accepted architecture ADRs.
- Verbatim authoritative specifications and checksum manifest.
- Completed M01-foundation.md status report with test evidence.

## Known risks

1. **Project license is undecided.** This is the only immediate owner decision that cannot be made safely by a coding agent.
2. **Public-repository licensing exposure.** No substantial external code should be accepted before the project license and contribution terms are explicit.
3. **Single-owner branch policy.** An approval requirement can deadlock a solo project; checks are mandatory, while owner review bypass must be explicit and audited.
4. **WPF CI is Windows-specific.** Keep domain code portable and retain a Windows build gate.
5. **Logging can leak sensitive paths.** Default redaction and no-model-content policy must be tested from the start.
6. **Over-engineering the bootstrap.** Milestone 1 must remain small; it establishes guardrails, not the CAD domain.
7. **Version drift.** Package/security state must be rechecked at implementation time; changes stay exact and documented.

## What must explicitly NOT be started yet

Do not begin any of the following during Milestone 1:

- OCCT download/build or native C++ code;
- P/Invoke or SafeHandle kernel wrappers;
- exact primitives or topology;
- Direct3D/Vortice renderer or viewport;
- document/feature graph;
- unit/expression system;
- sketch entities or solver;
- modelling commands;
- undo/redo for CAD actions;
- .tf720 persistence implementation;
- import/export providers;
- STEP/IGES/STL/OBJ/3MF/DXF/3DM work;
- topology naming implementation;
- healing/manufacturing validation;
- assemblies, surfaces, sheet metal, mesh conversion;
- proprietary-format SDK evaluation or integration;
- CAM, simulation, PCB, cloud, accounts, or collaboration;
- production installer, code signing, or branding polish.

# Acceptance Tests

## Promotion rule

The milestone owner publishes one signed-off evidence table:

| Field | Required value |
|---|---|
| Milestone | Number and title |
| Candidate commit | Full SHA |
| CI runs | URLs and conclusion |
| Automated tests | Passed/failed/skipped counts |
| Manual tests | IDs, environment, tester, result |
| Warnings | Zero, or milestone fails |
| Vulnerabilities | None above allowed policy |
| Coverage | Required threshold and actual |
| Known issues | Severity and containment |
| Technical debt | Explicit, bounded, owner |
| Architecture deviations | None, or approved ADR |
| Final status | PASS / PARTIAL / FAIL |

Only PASS permits the next milestone. PARTIAL is not a scheduling synonym for PASS.

## Regression policy

- Every geometry, solver, persistence, topology, import, and crash bug receives a regression test before closure.
- Golden files are immutable inputs with checksums and documented provenance.
- Expected numeric values state units and tolerance policy.
- “Approximately equal” helpers require an explicit domain tolerance; no generic epsilon is allowed.
- External round-trip qualification supplements, but does not replace, automated tests.
- Test failures preserve artifacts and diagnostics while excluding user CAD content.

## Initial golden-corpus plan

The corpus grows only when the responsible milestone begins:

| Milestone | Corpus |
|---|---|
| 3 | Analytic primitives, tiny/large dimensions, invalid parameters |
| 8 | Solvable, under-, over-, redundant-, near-singular sketches |
| 9–10 | Extrude/Boolean/hole/fillet/chamfer/shell/topology-change parts |
| 11 | Native format versions, interrupted/corrupt/path-traversal files |
| 12 | Invalid faces, open shells, non-manifold, tolerance outliers |
| 13 | STEP AP242 primitives, multi-body, assembly, NURBS, bad geometry |
| 14 | One licensed/provenance-recorded corpus per advertised format |

# Risks / Blockers

## Immediate blockers

1. **No project license.** Select a license before Milestone 1 can pass. If the intention is an open-source application with permissive reuse and an explicit patent grant, Apache-2.0 is a reasonable candidate. If commercial/proprietary distribution is intended, obtain legal guidance before accepting contributions. This report does not choose for the owner.
2. **No initial main ref.** The first minimal bootstrap commit must exist before branch rules and the Milestone 1 PR flow can operate.

## Major engineering risks

| Risk | Impact | Required mitigation |
|---|---|---|
| Native memory/exception boundary | Process crash or corruption | Versioned C ABI, handle registry, SafeHandle, sanitizer/leak tests |
| OCCT build and redistribution | Broken installs or license breach | Pinned vcpkg baseline, controlled DLL packaging, notices, clean-machine tests |
| Topological naming | Wrong downstream face/edge | First-class semantic references, history mapping, ambiguity failure, regression corpus |
| Constraint solver numerical behavior | Wrong dimensions/false constraint status | Residual/DOF reporting, scale-aware normalization, pathological fixtures |
| Tolerance escalation | Invalid “successful” geometry | Separate policies, hard maxima, audited healing, never tune tests by weakening tolerance |
| Atomic save/recovery | User data loss | Candidate file validation, atomic replacement, backup, journal and fault injection |
| WPF/child-HWND integration | DPI/input/overlay defects | Early viewport spike in Milestone 4, explicit airspace design, multi-monitor tests |
| GPU/driver failure | App instability | WARP fallback, disposable resources, device-loss recreation |
| Asynchronous recompute | Stale result overwrites new edit | Revision/cancellation tokens and commit-time revision check |
| Untrusted CAD inputs | Security compromise | Header validation, bounded parsing, archive safety, worker isolation, fuzzing |
| STEP semantic differences | Manufacturing mismatch | Fresh-context re-import plus independent external validation |
| Proprietary formats | Legal, cost, version coverage | Optional licensed providers; no core dependency or reverse engineering |
| Scope and schedule | Multi-year development | Hard milestone gates; finish the end-to-end core chain before breadth |
| Golden test data | Weak correctness evidence | Provenance-recorded corpus and independent expected measurements |

## Non-blocking research questions for their future milestones

- Benchmark OCCT 8.0.1 against the chosen Boolean/fillet regression corpus before declaring it the long-lived kernel baseline.
- Prototype HwndHost swap-chain input, DPI, accessibility, and overlay behavior in Milestone 4.
- Decide whether the sketch solver remains fully original C# or uses a separately licensed solver component after a formal license/performance evaluation.
- Define the .tf720 schema and migration policy before Milestone 11 implementation.
- Establish independent STEP validation applications and legally redistributable golden fixtures before Milestone 13.

# Recommended next action for Codex

Do **not** implement Milestone 2.

The next coding session should implement Milestone 1 exactly as specified, beginning with two owner confirmations:

1. Select the project license.
2. Confirm Windows 11 24H2+ x64 as the first formally supported/tested platform baseline.

Then Codex should:

1. create the minimal bootstrap commit;
2. create milestone/01-foundation;
3. implement only the listed projects/files and behaviors;
4. open a Milestone 1 pull request;
5. run and record every M1 automated/manual test;
6. report PASS, PARTIAL, or FAIL using the mandatory template;
7. stop.

Milestone 2 may begin only after the merged main commit independently passes the complete Milestone 1 gate.
