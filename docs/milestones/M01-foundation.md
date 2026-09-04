# Milestone 1 — Repository and engineering foundation

## MILESTONE STATUS

**Status: PASS — implementation, automated verification, required manual validation, repository protection, merge, and merged-`main` verification are complete.**

Milestone 1 is formally accepted under the approved owner amendment. The verified merged-`main` commit is `ff2bef7ad35ffa8fbaad313d551c2113433ade8d`.

### Owner gate amendment

On 2026-09-04 the repository owner approved `docs/specification/M1_OWNER_GATE_AMENDMENT_2026-09-04.md`.

For Milestone 1 only:

- M1-M05 is **OPTIONAL / DEFERRED** and non-blocking.
- M1-M06 is **OPTIONAL / DEFERRED** and non-blocking.
- The mandatory manual checks are M1-M01, M1-M02, M1-M03, M1-M04, M1-M07, and M1-M08.

The amendment changes test scheduling only. Offline normal operation and non-admin end-user operation remain product requirements and must be validated in later release/deployment qualification.

### Completed

- Apache-2.0 license selected by the owner and committed.
- Authoritative source documents retained with SHA-256 manifest and Windows checkout byte preservation.
- Five-project .NET/WPF solution, centralized exact dependencies, lock policy, warning/error policy, and x64 baseline.
- Portable Foundation contracts for diagnostics/results, typed IDs, product-local paths, bounded log policy, build metadata, and atomic startup sentinel.
- Minimal truthful WPF shell with Generic Host, bounded local JSON logging, lifecycle markers, and exception boundaries.
- Machine-readable diagnostics self-test with honest unimplemented-capability values.
- Unit, architecture, build-process, CI, CodeQL, governance, and engineering-script foundations.
- Six accepted architecture ADRs; no architecture deviation.
- Clean Windows verification completed successfully on the candidate code.
- Required Milestone 1 manual checks completed successfully under the owner amendment.
- Active `Protect main` repository ruleset verified with PR requirement, deletion/force-push protection, and required `managed-core`, `windows-x64`, and `codeql-csharp` checks.
- PR #1 merged to `main`; the resulting merge commit independently passed Milestone 1 CI and CodeQL.

### Automated verification evidence

The clean Windows verification was executed on commit `2c75984fa8180eef8b12fc76a763fe4e0d5efe59` with .NET SDK 10.0.400 / runtime 10.0.11.

| Gate | Result | Evidence |
|---|---|---|
| `eng/verify.ps1` | PASS | Clean Windows x64 execution |
| Foundation tests | PASS | 43 passed, 0 failed, 0 skipped |
| Architecture tests | PASS | 13 passed, 0 failed, 0 skipped |
| Foundation line coverage | PASS — 100% | Verification output |
| Foundation branch coverage | PASS — 100% | Verification output |
| Release x64 build | PASS — 0 build failures | Verification output |
| Formatting | PASS | 0 of 53 files required formatting |
| Deterministic Foundation build | PASS | Byte-identical repeated build; SHA-256 recorded by verification |
| Authoritative source hash check | PASS | M1-A08 passes on clean Windows checkout |

No required automated test was intentionally skipped or weakened.

### Manual validation evidence

**Tester:** repository owner / human tester  
**Environment:** Windows 11 Home 25H2 x64, OS build 26200.9168  
**.NET:** SDK 10.0.400; runtime 10.0.11  
**Tested code SHA:** `2c75984fa8180eef8b12fc76a763fe4e0d5efe59`  
**Later changes before merge:** documentation/governance only unless otherwise noted.

| ID | Status | Evidence |
|---|---|---|
| M1-M01 | PASS | Clean clone; `eng/verify.ps1` completed with all tests green, 100% Foundation line/branch coverage, and deterministic output |
| M1-M02 | PASS | Release `TFusion.App` opened the truthful minimal TFUSION-720 window with version/status and no CAD controls |
| M1-M03 | PASS | Normal close followed by restart succeeded cleanly |
| M1-M04 | PASS | Process was force-terminated by PID; restart succeeded and JSON log recorded `Previous session state required attention` with `Unclean=true` and `InvalidMarker=false` |
| M1-M05 | OPTIONAL / DEFERRED | Non-blocking by owner amendment; retained for later release/deployment offline qualification |
| M1-M06 | OPTIONAL / DEFERRED | Non-blocking by owner amendment; retained for later installer/standard-user qualification |
| M1-M07 | PASS — controlled equivalent | Product-local storage root was deliberately made unavailable by replacing the expected directory with a file after backing up existing state; app produced a controlled `TFUSION-720 startup failure`, created no fallback directories/files in the repository/current directory, and original AppData state was restored afterward |
| M1-M08 | PASS | Log was valid JSONL, one file at 9686 bytes, no file over 10 MB, and no matches for full `C:\Users\` paths, password/passwd/secret/token/API-key patterns |

The M1-M07 execution used an equivalent controlled storage-unavailable condition rather than changing ACLs in a disposable profile. It exercised the same startup storage-failure path and verified the required no-fallback behavior. No production data was lost; the original product-local storage was restored.

### Repository protection

Repository ruleset `Protect main` is active and targets the default branch. It:

- blocks branch deletion;
- blocks non-fast-forward/force-push updates;
- requires pull-request-based changes;
- requires `managed-core`, `windows-x64`, and `codeql-csharp` status checks.

The ruleset currently has no bypass actor. This is stricter than the roadmap's suggested emergency-owner bypass and does not weaken the protected-branch gate.

### Merged-main verification evidence

**Merged-main commit:** `ff2bef7ad35ffa8fbaad313d551c2113433ade8d`

| Gate | Result | Evidence |
|---|---|---|
| Milestone 1 CI | PASS | [Run 33863189076](https://github.com/Arcion999/TFUSION-720/actions/runs/33863189076) (`push` on merged `main`) |
| `managed-core` | PASS | Job `100992051040` in run 33863189076 |
| `windows-x64` | PASS | Job `100992051462` in run 33863189076 |
| CodeQL | PASS | [Run 33863189107](https://github.com/Arcion999/TFUSION-720/actions/runs/33863189107) (`push` on merged `main`) |
| `codeql-csharp` | PASS | Job `100992051182` in run 33863189107 |

The commit SHA and run/job conclusions were re-read from GitHub before this final status update. M1-M05 and M1-M06 remain optional/deferred; neither is represented as executed.

### Signed-off PASS/FAIL checklist

- [x] Required files and five project responsibilities exist.
- [x] Owner-selected Apache-2.0 license is committed.
- [x] Authoritative source files match `SOURCES.sha256`.
- [x] Every M1-U, M1-A, and M1-B automated test has passed on the tested candidate.
- [x] Every **mandatory** M1-M manual test has passed under the 2026-09-04 owner amendment.
- [x] M1-M05 and M1-M06 are explicitly documented as optional/deferred rather than falsely marked PASS.
- [x] PR-head CI/CodeQL completed successfully before merge.
- [x] CI/CodeQL is green on merged `main` commit `ff2bef7ad35ffa8fbaad313d551c2113433ade8d`.
- [x] Release x64 build has zero warnings under the verification gate.
- [x] Coverage is at least 90% line and 85% branch; actual Foundation coverage is 100% / 100%.
- [x] Locked restore and deterministic Foundation output pass.
- [x] High/critical dependency audit is clean under the verification gate.
- [x] Locked third-party inventory has been reconciled.
- [x] `main` rules require PR/checks and prevent force-push/deletion.
- [x] WPF shell contains no false CAD functionality.
- [x] No unapproved architecture deviation exists.

### Gate closure

All mandatory Milestone 1 gates are complete under the approved owner amendment. Milestone 2 may begin from the latest green `main` after this documentation-only closure is merged and its resulting `main` checks are green.

### Known limitations / deferred qualification

- M1-M05 offline manual qualification is deferred by owner decision; the offline-first product requirement remains unchanged.
- M1-M06 standard-user manual qualification is deferred by owner decision; normal end-user operation must still not require administrator rights.
- M1-M07 used a controlled equivalent storage-unavailable setup rather than a separate disposable Windows profile.
- Merged-`main` CI and CodeQL evidence is recorded above.

### Technical debt introduced

No product-code debt is accepted by this amendment. M1-M05 and M1-M06 are explicit deferred qualification tasks, not claims of untested success.

### Files/modules changed

Repository policy and governance; `TFusion.Foundation`; `TFusion.App`; `TFusion.Diagnostics`; both test projects; engineering scripts; CI/CodeQL; architecture, specification-amendment, and milestone documents.

### Architecture deviations

None. The owner amendment changes only Milestone 1 manual-gate scheduling.

### Recommended next milestone

After this documentation-only closure passes its protected PR and merged-`main` checks, begin Milestone 2 — OCCT native bridge foundation — on a dedicated branch from that latest green `main`.
