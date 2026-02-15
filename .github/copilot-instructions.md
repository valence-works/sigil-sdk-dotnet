# Copilot Instructions — Sigil SDK

This repository is specification-driven.

Rules:
- Do not generate production code unless a corresponding spec exists in /specs.
- Always reference the relevant spec number in comments.
- Do not introduce breaking API changes without explicit versioning notes.
- Keep verification logic deterministic and side-effect free.
- Avoid platform-specific dependencies in core validation logic.
- Favor explicit over clever.
- Prefer composable design over monolithic classes.
