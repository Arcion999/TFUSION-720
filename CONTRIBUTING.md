# Contributing to TFUSION-720

TFUSION-720 advances through hard milestone gates. Read the three files in `docs/specification/` and the accepted ADRs before proposing a change.

## Required workflow

1. Work on a branch; do not push directly to `main`.
2. Keep the change within the active milestone. Do not add empty future assemblies or UI that implies unavailable behavior.
3. Record any proposed architecture deviation in an ADR and obtain review before implementing it.
4. Add tests for success and failure paths. Do not weaken tolerances, suppress warnings broadly, skip tests, or convert a failure into a fake success.
5. Run `./eng/verify.ps1` from PowerShell 7 on the supported Windows baseline.
6. Complete the pull-request checklist and attach manual-test evidence required by the milestone.

Dependencies require central, exact, non-prerelease versions, a committed lock-file update, license/provenance review in `THIRD_PARTY_NOTICES.md`, and the normal vulnerability scan. Normal CAD operation must remain offline-capable.

User data safety and geometric correctness take priority over schedule, performance, UI polish, and feature count. Report errors explicitly; never silently discard or repair engineering data.
