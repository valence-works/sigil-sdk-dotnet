# sigil-sdk-dotnet

.NET SDK for validating Sigil proof envelopes using zero-knowledge proofs.

## Quick Start

Integrate Sigil validation into your .NET application with 3 lines:

```csharp
builder.Services.AddSigilValidation();
// Inject ILicenseValidator into your services
var result = await validator.ValidateAsync(envelopeJson);
```

**[→ Full DI Integration Guide](docs/DI_INTEGRATION.md)**

## Documentation

- **[DI Integration Guide](docs/DI_INTEGRATION.md)** - Comprehensive setup and configuration guide
- **[Architecture](docs/architecture.md)** - System architecture and design decisions
- **[Domain Model](docs/domain-model.md)** - Core domain concepts and relationships
- **[Vocabulary](docs/vocabulary.md)** - Terminology and definitions
- **[Vision](docs/vision.md)** - Project goals and roadmap

## Features

- **Drop-in Validator**: Single-line registration with ASP.NET Core dependency injection
- **Secure Defaults**: Production-ready configuration with no sensitive data logging
- **Extensible**: Add custom proof systems and statement handlers
- **Fast Startup**: Schema compilation < 100ms, service overhead < 5MB
- **Standards Compliant**: Full support for Sigil Proof Envelope format (Spec 001)

## Installation

```bash
dotnet add package Sigil.Sdk
```

## Specifications

This project is specification-driven. See [specs/](specs/) for detailed specifications.

Current specs:
- [001 - Proof Envelope Format](specs/001-proof-envelope-format/)
- [002 - SDK Validation API](specs/002-sdk-validation-api/)
- [003 - DI Integration & Defaults](specs/003-di-integration/)

## Samples

See [samples/MinimalDiSample](samples/MinimalDiSample) for a complete working example demonstrating:
- 3-5 line setup
- ASP.NET Core integration
- REST API with Swagger/OpenAPI
- Custom diagnostics configuration

## License

See [LICENSE](LICENSE) file for details.

---

**Spec 003 (SC-001)**: <5 minutes integration time with ≤5 lines of code ✓
