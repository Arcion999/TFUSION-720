# Milestone 1 — Repository and engineering foundation

## MILESTONE STATUS

**Status: PARTIAL — implementation complete; verification evidence pending.**

Milestone 2 is blocked. This report must be updated with immutable run/commit evidence and owner-performed Windows manual results before status can become `PASS`.

### Completed

- Apache-2.0 license selected by the owner and committed.
- Authoritative source documents committed verbatim with SHA-256 manifest.
- Five-project .NET/WPF solution, centralized exact dependencies, lock policy, warning/error policy, and x64 baseline.
- Portable Foundation contracts for diagnostics/results, typed IDs, product-local paths, bounded log policy, build metadata, and atomic startup sentinel.
- Minimal truthful WPF shell with Generic Host, bounded local JSON logging, lifecycle markers, and exception boundaries.
- Machine-readable diagnostics self-test with honest unimplemented-capability values.
- Unit, architecture, build-process, CI, CodeQL, governance, and engineering-script foundations.
- Six accepted architecture ADRs; no architecture deviation.

### Automated tests

| Group | Required | Current result | Evidence |
|---|---:|---|---|
| M1-U01–M1-U12 | 12 | Pending CI | PR checks |
| M1-A01–M1-A12 | 12 | Pending CI | PR checks |
| M1-B01–M1-B08 | 8 | Pending CI | PR checks |
| Foundation line coverage | ≥90% | Pending CI | Cobertura artifact |
| Foundation branch coverage | ≥85% | Pending CI | Cobertura artifact |

No required test is intentionally skipped.

### Manual checks

M1-M01 through M1-M08 are **NOT EXECUTED**. They require a clean Windows 11 24H2-or-newer x64 VM and an identified human tester. Record tester, OS build, SDK/runtime, account type, commit SHA, exact command, result, and evidence for every case.

### Signed-off PASS/FAIL checklist

- [x] Required files and five project responsibilities exist.
- [x] Owner-selected Apache-2.0 license is committed.
- [x] Three authoritative files match `SOURCES.sha256`.
- [ ] Every M1-U, M1-A, M1-B, and M1-M test has passed.
- [ ] CI is green on the PR head and merged `main`.
- [ ] Release x64 build has zero warnings.
- [ ] Coverage is at least 90% line and 85% branch.
- [ ] Locked restore and deterministic Foundation output pass.
- [ ] High/critical dependency audit is clean.
- [x] Locked third-party inventory has been reconciled.
- [ ] `main` rules require PR/checks and prevent force-push/deletion.
- [x] WPF shell contains no false CAD functionality.
- [x] No unapproved architecture deviation exists.

### Known issues

- This execution environment has no .NET SDK, PowerShell 7, Windows desktop runtime, or interactive Windows session; it cannot locally compile or perform the manual gate.
- Repository protection must be verified/configured with owner administrative permissions.

### Technical debt introduced

None accepted. Pending verification is gate work, not deferred product debt.

### Files/modules changed

Repository policy and governance; `TFusion.Foundation`; `TFusion.App`; `TFusion.Diagnostics`; both test projects; engineering scripts; CI/CodeQL; architecture and milestone documents.

### Architecture deviations

None.

### Recommended next milestone

Do not begin Milestone 2. First obtain green PR CI, reconcile package locks/notices, run and sign M1-M01–M1-M08 on the required Windows VM, configure/verify `main` rules, merge only after the gate passes, then verify merged `main` CI.
