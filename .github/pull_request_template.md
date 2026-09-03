## Scope

- [ ] Change is limited to the active milestone.
- [ ] No fake, disabled, or placeholder product capability was added.
- [ ] Authoritative specifications and accepted ADRs were followed.
- [ ] Any proposed architecture deviation has a reviewed ADR before implementation.

## Verification

- [ ] `./eng/verify.ps1` passes on the supported Windows x64 baseline.
- [ ] New success and failure paths have automated tests.
- [ ] Required manual tests include tester, OS/build, commit SHA, command, and evidence.
- [ ] No test was skipped, removed, or weakened to obtain a green result.
- [ ] Build is warning-free and coverage remains above the active gate.

## Safety and dependencies

- [ ] User/document data cannot be silently discarded or overwritten.
- [ ] Failures return/log structured diagnostics; no expected exception is ignored.
- [ ] Dependency changes are exact, centrally managed, locked, audited, and recorded in `THIRD_PARTY_NOTICES.md`.
- [ ] Normal CAD operation remains offline-capable and no telemetry/cloud dependency was added.

## Evidence and limitations

Describe implemented behavior, tests, manual evidence, known issues, technical debt, changed modules, and deviations. A mandatory unmet criterion means the milestone remains `PARTIAL` or `FAIL`.
