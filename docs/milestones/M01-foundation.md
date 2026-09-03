# Milestone 1 — Repository and engineering foundation

## MILESTONE STATUS

**Status: PARTIAL — implementation and automated verification complete; owner/manual gate work remains.**

Milestone 2 remains blocked until the manual Windows validation, `main` protection, merge, and merged-`main` CI gate are complete.

### Completed

- Apache-2.0 license selected by the owner and committed.
- Authoritative source documents committed verbatim with SHA-256 manifest.
- Five-project .NET/WPF solution, centralized exact dependencies, lock policy, warning/error policy, and x64 baseline.
- Portable Foundation contracts for diagnostics/results, typed IDs, product-local paths, bounded log policy, build metadata, and atomic startup sentinel.
- Minimal truthful WPF shell with Generic Host, bounded local JSON logging, lifecycle markers, and exception boundaries.
- Machine-readable diagnostics self-test with honest unimplemented-capability values.
- Unit, architecture, build-process, CI, CodeQL, governance, and engineering-script foundations.
- Six accepted architecture ADRs; no architecture deviation.
- PR-head Milestone 1 CI is green on commit `72fe5acb0303f49578ea2e0d8ff2af584f5941e0`.
- PR-head CodeQL is green on the same commit.

### Automated verification evidence

| Gate | Result | Evidence |
|---|---|---|
| Managed-core CI | PASS | GitHub Actions run `33809585583`, job `100828031668` |
| Windows x64 CI | PASS | GitHub Actions run `33809585583`, job `100828031897` |
| Full `eng/verify.ps1` | PASS | Windows x64 job step `Full verification` |
| Diagnostics success/invalid-argument contract | PASS | Windows x64 job step `Diagnostics success and argument contracts` |
| CodeQL C# | PASS | GitHub Actions run `33809585696`, job `100828032096` |
| M1-U01–M1-U12 | PASS | Foundation test suite in green CI |
| M1-A01–M1-A12 | PASS | Architecture test suite in green CI |
| M1-B01–M1-B08 | PASS | Build/process gates in green CI |
| Foundation line coverage | PASS — 100% | CI verification output |
| Foundation branch coverage | PASS — 100% | CI verification output |
| Release x64 warnings/errors | PASS — 0 warnings / 0 errors | CI verification output |
| Locked restore | PASS | Managed-core, Windows x64, and CodeQL runs |
| Deterministic Foundation build | PASS | `eng/verify.ps1` in Windows x64 job |
| High/critical dependency audit | PASS | `eng/verify.ps1` in Windows x64 job |

No required automated test is intentionally skipped or weakened.

### Manual checks

M1-M01 through M1-M08 are **NOT EXECUTED**. The authoritative roadmap requires these on a clean Windows 11 24H2-or-newer x64 environment with an identified human tester. Record tester, OS build, SDK/runtime, account type, commit SHA, exact command, result, and evidence for every case.

| ID | Status |
|---|---|
| M1-M01 | NOT EXECUTED |
| M1-M02 | NOT EXECUTED |
| M1-M03 | NOT EXECUTED |
| M1-M04 | NOT EXECUTED |
| M1-M05 | NOT EXECUTED |
| M1-M06 | NOT EXECUTED |
| M1-M07 | NOT EXECUTED |
| M1-M08 | NOT EXECUTED |

### Repository protection

`main` is currently **not protected** and the repository currently has no ruleset enforcing the Milestone 1 gate. Before merge, configure `main` so that pull requests and designated CI checks are required and force-push/deletion are prevented.

### Signed-off PASS/FAIL checklist

- [x] Required files and five project responsibilities exist.
- [x] Owner-selected Apache-2.0 license is committed.
- [x] Three authoritative files match `SOURCES.sha256`.
- [x] Every M1-U, M1-A, and M1-B automated test has passed on the PR head.
- [ ] Every M1-M manual test has passed.
- [x] CI is green on the PR head.
- [ ] CI is green on merged `main`.
- [x] Release x64 build has zero warnings.
- [x] Coverage is at least 90% line and 85% branch.
- [x] Locked restore and deterministic Foundation output pass.
- [x] High/critical dependency audit is clean.
- [x] Locked third-party inventory has been reconciled.
- [ ] `main` rules require PR/checks and prevent force-push/deletion.
- [x] WPF shell contains no false CAD functionality.
- [x] No unapproved architecture deviation exists.

### Remaining gate sequence

1. Execute and record M1-M01 through M1-M08 on the required Windows 11 x64 environment.
2. Configure and verify `main` branch/ruleset protection.
3. Merge PR #1 only after items 1 and 2 pass.
4. Verify Milestone 1 CI and CodeQL on merged `main`.
5. Update this report to `PASS` with merged commit/run evidence.
6. Only then begin Milestone 2.

### Known limitations

- The connected GitHub integration can inspect repository protection but does not expose an administrative mutation for configuring branch protection/rulesets.
- This assistant session does not provide an interactive clean Windows 11 desktop/VM, so the human manual gate cannot be truthfully executed here.

### Technical debt introduced

None accepted. Remaining work is milestone gate verification, not deferred product debt.

### Files/modules changed

Repository policy and governance; `TFusion.Foundation`; `TFusion.App`; `TFusion.Diagnostics`; both test projects; engineering scripts; CI/CodeQL; architecture and milestone documents.

### Architecture deviations

None.

### Recommended next milestone

Do not begin Milestone 2 until the remaining manual/repository/merged-main gates above are complete and this report is changed to `PASS`.
