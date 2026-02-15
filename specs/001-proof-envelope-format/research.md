# Research: Proof Envelope Format

## Decisions

### Envelope versioning
- Decision: Require `envelopeVersion` to equal "1.0" for v1.
- Rationale: Fixed version values avoid ambiguous validators and ensure deterministic behavior.
- Alternatives considered: Semantic range matching (e.g., accept 1.x), or free-form version strings.

### Statement identifier format
- Decision: Require URN-like identifiers using the pattern `^[a-zA-Z][a-zA-Z0-9+.-]*:.*$`.
- Rationale: Aligns with common URN/URI identifier conventions while allowing vendor namespaces.
- Alternatives considered: Strict RFC 8141 URN parsing or free-form strings.

### Base64 encoding
- Decision: Require standard base64 with `A-Z`, `a-z`, `0-9`, `+`, `/` and `=` padding.
- Rationale: Maximizes interoperability across platforms and libraries.
- Alternatives considered: URL-safe base64 or unpadded base64.

### Public input feature naming
- Decision: Enforce lowercase kebab-case for `features` entries with regex `^[a-z0-9]+(?:-[a-z0-9]+)*$`.
- Rationale: Ensures stable, readable identifiers and matches requirement language.
- Alternatives considered: snake_case or mixed-case identifiers.

### Expiry evaluation
- Decision: Compare `expiresAt` against current UTC time and fail with `Expired` when in the past.
- Rationale: UTC comparison avoids timezone inconsistencies in offline verification.
- Alternatives considered: Local time comparison or verifier-supplied clock input.
