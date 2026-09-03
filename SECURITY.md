# Security Policy

TFUSION-720 is pre-alpha and has no supported production release. Security fixes are nevertheless handled as high priority, especially issues affecting local file safety, unsafe native loading, archive extraction, or dependency integrity.

Do not disclose a suspected vulnerability in a public issue. Use the repository's **Security → Report a vulnerability** private reporting flow. Include the affected commit, Windows build, reproduction steps, expected and observed behavior, and impact. Do not include real CAD files, secrets, access tokens, personal paths, or other sensitive data; use a minimal synthetic fixture.

Dependencies are restored only from the source declared in `NuGet.config`, with exact centrally managed versions and committed lock files. CI audits direct and transitive packages for high/critical advisories. GitHub Actions are pinned to immutable commit SHAs.
