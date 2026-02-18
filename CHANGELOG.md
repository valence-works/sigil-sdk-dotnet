# Changelog

All notable changes to the Sigil SDK for .NET will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Spec 003: DI Integration & Defaults

- **Principal Feature**: `AddSigilValidation()` extension method for drop-in dependency injection integration
  - Single-line registration: `services.AddSigilValidation()`
  - Optional configuration delegate for custom proof systems, statement handlers, and diagnostics
  - All services registered as singletons for optimal performance
  - Schema compilation happens once at startup (< 100ms)
  - Memory overhead < 5MB with default configuration

- **Extension Points**:
  - `ValidationOptions.AddProofSystem(identifier, verifier)` - Register custom proof system verifiers
  - `ValidationOptions.AddStatementHandler(handler)` - Register custom statement handlers
  - Duplicate identifiers throw `InvalidOperationException` immediately at registration time
  - Registries are immutable after `BuildServiceProvider()` is called (FR-015)

- **Configuration Options**:
  - `ValidationOptions.EnableDiagnostics` (default: `false`) - Control diagnostic information collection
  - `ValidationOptions.LogFailureDetails` (default: `false`) - Control sensitive data logging (secure default)

- **Error Handling**:
  - Calling `AddSigilValidation()` multiple times throws `InvalidOperationException` (FR-011)
  - Invalid identifiers (empty/whitespace) throw `ArgumentException` immediately (FR-007, FR-008)
  - Runtime modification of registries throws `InvalidOperationException` with clear guidance (FR-015)
  - Custom verifier construction errors are wrapped in `InvalidOperationException` (FR-016b)

- **Documentation**:
  - [DI Integration Guide](docs/DI_INTEGRATION.md) - Comprehensive setup and configuration guide
  - [Architecture](docs/architecture.md) - Updated with DI integration strategy and service lifetime decisions
  - [Migration Guide](MIGRATION.md) - Migration documentation (initial release, no migrations required)
  - Minimal sample in [samples/MinimalDiSample](samples/MinimalDiSample) demonstrating 3-5 line setup

- **Testing**:
  - 50+ unit tests covering all DI scenarios, error handling, and configuration options
  - Integration tests for ASP.NET Core, console app, and worker service scenarios
  - Performance tests verifying schema compilation < 100ms and memory overhead < 5MB
  - Immutability enforcement tests for registries

### Changed

- **No Breaking Changes**: All changes are additive and backward compatible

### Deprecated

- `AddSigilValidation(IEnumerable<KeyValuePair<string, IProofSystemVerifier>>, IEnumerable<KeyValuePair<string, IStatementHandler>>, Action<ValidationOptions>?)` 
  - Legacy overload maintained for backward compatibility
  - Use `AddSigilValidation()` or `AddSigilValidation(Action<ValidationOptions>)` instead
  - Will be removed in a future major version

### Security

- **Secure Defaults**: `LogFailureDetails` defaults to `false` to prevent sensitive data leakage (FR-010)
- **No Proof Bytes in Logs**: SDK enforces constitution constraint that `proofBytes` never appear in logs or error messages (FR-016)
- **Immutable Registries**: Prevents runtime tampering with proof system or statement validation logic (FR-015)

---

## [0.1.0] - Initial Release (Specs 001-002)

### Added

- **Spec 001: Proof Envelope Format**
  - JSON schema definition for proof envelopes
  - Schema validation for envelope structure
  - Support for envelope version 1.0

- **Spec 002: SDK Validation API**
  - `ILicenseValidator` public interface for validation
  - `ValidateAsync()` method accepting JSON proof envelopes
  - `LicenseValidationResult` with status, failure codes, and claims
  - Deterministic validation with no side effects
  - Proof system registry (`IProofSystemRegistry`) with custom verifier support
  - Statement registry (`IStatementRegistry`) with custom handler support
  - Schema validator with compiled validation
  - Clock abstraction (`IClock`) for time-based validation

### Security

- Validation logic is side-effect free and deterministic
- No sensitive data logging by default
- Immutable proof envelope after validation

---

[Unreleased]: https://github.com/ValenceWorks/sigil-sdk-dotnet/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/ValenceWorks/sigil-sdk-dotnet/releases/tag/v0.1.0
