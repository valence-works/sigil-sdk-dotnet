# Quickstart — Proof System Verifier Contract (Spec 005)

This quickstart describes how to validate conformance to the proof-system verifier contract.

## Preconditions

- Spec 001 envelope format and Spec 002 validator pipeline are already implemented.
- A statement handler can produce verification context for the selected statement.
- The proof verifier registry is configured via DI and immutable after startup.

## 1) Register proof verifiers

- Register exactly one verifier for each canonical `proofSystem` ID.
- Include `midnight-zk-v1` as the initial supported proof system.
- Ensure duplicate registration for the same canonical ID fails before runtime validation.

## 2) Run deterministic verification checks

- Submit identical requests (`proofBytes` + verification context + registry config) multiple times.
- Confirm identical status and failure code outcomes for every repetition.
- Confirm no network calls are required during verification.

## 3) Verify failure mapping

- Unsupported `proofSystem` -> status `Unsupported`.
- Supported system + proof verification failure -> status `Invalid`.
- Supported system + proof/context incompatibility -> status `Invalid`.
- Internal verifier fault -> status `Error`.
- Cancellation -> propagated cancellation (no status mapping).

## 4) Verify stage precedence (first failing stage wins)

- Malformed/schema-invalid envelopes must fail before proof verification.
- Unsupported identifier failures must not be overridden by proof-stage results.
- Proof-stage results must not override earlier stage failures.

## 5) Verify confidentiality and diagnostics

- Enable diagnostics and execute success/failure cases.
- Confirm no logs, diagnostics, exceptions, or result payloads include raw `proofBytes`.
- Confirm diagnostics provide only redacted, minimal troubleshooting metadata.

## 6) Validate performance target alignment

- Use a representative offline corpus of envelopes <= 10 KB.
- Measure end-to-end validation latency across at least 100 runs after warm-up.
- Confirm p95 latency remains below 1 second.
