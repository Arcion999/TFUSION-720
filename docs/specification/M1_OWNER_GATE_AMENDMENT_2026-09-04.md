# Milestone 1 Owner Gate Amendment — 2026-09-04

**Status:** APPROVED by repository owner for Milestone 1 only.

## Purpose

This is a visible owner-directed amendment to the Milestone 1 manual acceptance gate in `TFUSION-720_Definitive_Milestone_Roadmap.md`. It preserves the original roadmap file and its provenance while explicitly superseding only the Milestone 1 treatment of M1-M05 and M1-M06.

## Decision

For **Milestone 1 only**:

- **M1-M05 — offline launch/diagnostics** is **OPTIONAL / DEFERRED** and is not required for Milestone 1 promotion.
- **M1-M06 — standard-user-account storage validation** is **OPTIONAL / DEFERRED** and is not required for Milestone 1 promotion.
- The mandatory Milestone 1 manual checks are therefore **M1-M01, M1-M02, M1-M03, M1-M04, M1-M07, and M1-M08**.
- Not executing M1-M05 or M1-M06 is not considered a skipped mandatory test and does not by itself make Milestone 1 PARTIAL or FAIL.

## Requirements that remain unchanged

This amendment changes **test scheduling**, not the product requirements:

- TFUSION-720 remains an offline-first application; normal CAD operation must not require a network connection.
- TFUSION-720 must not require administrator rights for normal end-user operation.
- If evidence shows that the application actually requires network access or administrator rights, that remains a defect and must not be hidden by the optional status of these two tests.
- M1-M05 is retained for later release/deployment qualification.
- M1-M06 is retained for later installer/packaging/release qualification.
- All automated Milestone 1 gates, all other mandatory manual gates, branch protection, and merged-`main` CI requirements remain unchanged.

## Interpretation of the roadmap

For Milestone 1, roadmap phrases such as “all M1-M tests” or “every manual test” are to be read as **all mandatory Milestone 1 manual tests**, with M1-M05 and M1-M06 explicitly excluded by this amendment.

This amendment does not modify the architecture, precision goals, offline product goal, data-safety requirements, or any later milestone.
