# Feature Specification: DI Integration & Defaults

**Feature Branch**: `003-di-integration`  
**Created**: 2026-02-17  
**Status**: Draft  
**Input**: User description: "DI Integration & Defaults - Goal: drop-in validator for real apps. Includes: services.AddSigilValidation(...) (or AddSigil(...) but keep naming explicit). Registers: ISigilValidator, registries (immutable), schema validator compiled once, clock abstraction default, options for diagnostics/logging. Clear extension points: add proof systems, add statements. Minimal sample in /samples. Deliverable: a .NET app can validate with 3-5 lines of setup."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Validator Setup (Priority: P1)

A .NET developer building an application that receives proof envelopes needs to add validation capability. They install the Sigil SDK NuGet package, add a single method call to their service registration, and can immediately validate envelopes without additional configuration.

**Why this priority**: This is the core MVP - the "drop-in validator" that enables developers to integrate Sigil validation with minimal friction. Without this, the SDK cannot be used in real applications.

**Independent Test**: Install the SDK package, call `services.AddSigilValidation()` in `Program.cs` or `Startup.cs`, inject `ISigilValidator` into a controller or service, and validate a proof envelope. The test passes if validation completes with expected results using only default configuration.

**Acceptance Scenarios**:

1. **Given** a .NET application with ASP.NET Core dependency injection configured, **When** developer calls `services.AddSigilValidation()` during service registration, **Then** all required validation services are registered and available for injection
2. **Given** services are registered, **When** developer injects `ISigilValidator` into a class constructor, **Then** the validator instance is provided by the DI container with all dependencies resolved
3. **Given** a validator instance, **When** developer calls the validation method with a proof envelope, **Then** validation executes using default configuration (schema validation, empty registries, system clock, standard logging)
4. **Given** a new .NET project, **When** developer follows the 3-5 line setup pattern from documentation, **Then** validation works immediately without errors or additional configuration

---

### User Story 2 - Custom Diagnostics Configuration (Priority: P2)

A developer integrating Sigil validation into a production application needs to control how validation failures are logged and diagnosed. They configure logging verbosity, enable or disable specific diagnostic features, and control what information appears in logs to meet their security and compliance requirements.

**Why this priority**: Production applications need control over logging behavior for security (e.g., not logging sensitive data), performance (controlling verbosity), and compliance (meeting audit requirements). This is essential for production readiness but not required for basic MVP functionality.

**Independent Test**: Configure custom logging options during registration (e.g., `options.LogFailureDetails = false`), perform validation, and verify that logs respect the configured settings. The test passes if logs match the specified configuration.

**Acceptance Scenarios**:

1. **Given** a developer registers services with `AddSigilValidation(options => { options.LogFailureDetails = false; })`, **When** validation failures occur, **Then** detailed failure information is not written to logs
2. **Given** custom diagnostic options are configured, **When** validation executes, **Then** the validator respects these options for all operations
3. **Given** production security requirements, **When** developer disables sensitive data logging, **Then** proof bytes and other sensitive data never appear in logs or diagnostic output

---

### User Story 3 - Extension with Custom Systems (Priority: P3)

A developer working with custom proof systems or statement types needs to extend the SDK to support their specific use cases. They register custom proof system verifiers and statement handlers during service setup, and these extensions integrate seamlessly with the validation pipeline.

**Why this priority**: This enables advanced scenarios and custom integrations, making the SDK extensible. However, the SDK can function fully for standard use cases without this capability.

**Independent Test**: Register a custom proof system verifier using `options.AddProofSystem(...)`, validate an envelope containing that proof system, and verify the custom verifier is invoked. The test passes if custom extensions are integrated into the validation flow.

**Acceptance Scenarios**:

1. **Given** a developer has a custom proof system verifier, **When** they call `options.AddProofSystem(customIdentifier, verifier)` during registration, **Then** the verifier is added to the proof system registry
2. **Given** a custom statement handler, **When** developer registers it via `options.AddStatementHandler(handler)`, **Then** the handler is available during validation
3. **Given** custom extensions are registered, **When** validation encounters matching proof systems or statements, **Then** the custom implementations are invoked correctly
4. **Given** multiple developers on a team, **When** they review extension registration code, **Then** the extension points are clear and self-documenting

---

### Edge Cases

- When `AddSigilValidation()` is called multiple times on the same service collection, it throws `InvalidOperationException` with a clear error message
- When registering duplicate proof systems with the same identifier, the system throws `InvalidOperationException` immediately during registration
- When a developer tries to modify registries after service registration completes, the system throws `InvalidOperationException`
- When required dependencies (like `IOptions<ValidationOptions>`) are missing or misconfigured, the system throws `InvalidOperationException` during registration or container build
- When a custom proof system verifier throws during registration, the system throws `InvalidOperationException` with context and fails fast

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a service collection extension method named `AddSigilValidation` that registers all required validation services
- **FR-002**: The extension method MUST register `ISigilValidator` as a service available for dependency injection
- **FR-003**: The extension method MUST register immutable proof system and statement registries with empty default state, allowing developers to explicitly add systems via configuration
- **FR-004**: The extension method MUST register and compile the schema validator exactly once, ensuring optimal startup performance
- **FR-005**: The extension method MUST register a default clock implementation (system clock) for time-based validation operations
- **FR-006**: The extension method MUST accept an optional configuration delegate for `ValidationOptions` to customize diagnostics and logging behavior
- **FR-007**: The configuration API MUST provide a clear method for adding custom proof system verifiers (e.g., `options.AddProofSystem(identifier, verifier)`) and MUST throw `InvalidOperationException` immediately if a duplicate identifier is registered
- **FR-008**: The configuration API MUST provide a clear method for adding custom statement handlers (e.g., `options.AddStatementHandler(handler)`) and MUST throw `InvalidOperationException` immediately if a duplicate identifier is registered
- **FR-009**: All registered services MUST use appropriate DI lifetimes (singleton for immutable registries and schema validator, scoped or transient as appropriate for stateful services)
- **FR-010**: Default configuration MUST be production-ready with secure defaults (no sensitive data logging, reasonable performance settings)
- **FR-011**: The extension method MUST throw `InvalidOperationException` with a clear, actionable error message if called multiple times on the same service collection
- **FR-012**: Documentation MUST include a minimal working example requiring no more than 5 lines of setup code
- **FR-013**: A sample project in `/samples` MUST demonstrate the typical integration pattern for a .NET application
- **FR-014**: Schema validator registration MUST use compiled schema validation for runtime performance (ref: spec 001 & 002 requirements for schema validation)
- **FR-015**: Registry registration MUST enforce immutability after service container is built by throwing `InvalidOperationException` on any modification attempt (ref: SDK constitution on registry immutability)
- **FR-016**: Logging configuration MUST respect the no-proofBytes-logging constraint from the SDK constitution
- **FR-016a**: Missing or misconfigured required dependencies MUST cause `InvalidOperationException` during registration or container build with a clear, actionable message
- **FR-016b**: Exceptions thrown by custom proof system verifiers during registration MUST be wrapped or surfaced as `InvalidOperationException` with clear context and must fail fast
- **FR-017**: The extension method MUST integrate with ASP.NET Core dependency injection and support generic .NET host scenarios

## Assumptions

- The Proof Envelope format is defined and stable (Spec 001)
- The validation API and its contracts are defined (Spec 002)
- Target .NET version supports modern dependency injection patterns (Microsoft.Extensions.DependencyInjection)
- Developers using this integration have basic familiarity with .NET dependency injection concepts
- Schema compilation is a one-time operation that can be performed at startup without significant performance impact
- Applications using this integration can tolerate startup-time initialization of validation infrastructure

## Clarifications

### Session 2026-02-17

- Q: What should happen when `AddSigilValidation()` is called multiple times on the same service collection? → A: Throw InvalidOperationException on second call with clear error message
- Q: How should the system handle registering duplicate proof systems with the same identifier? → A: Throw InvalidOperationException immediately when duplicate detected during registration
- Q: What should occur if a developer tries to modify registries after the service container is built? → A: Throw InvalidOperationException when modification is attempted after container build
- Q: How should the system behave if required dependencies (like `IOptions<ValidationOptions>`) are missing or misconfigured? → A: Throw InvalidOperationException during registration or container build with clear guidance
- Q: What should happen when a custom proof system verifier throws an exception during registration? → A: Throw InvalidOperationException immediately with context

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can integrate Sigil validation into a new .NET application in under 5 minutes using no more than 5 lines of setup code
- **SC-002**: Schema validator compilation during service registration completes in under 100 milliseconds for typical applications
- **SC-003**: 90% of developers successfully integrate the SDK on their first attempt following the minimal example documentation
- **SC-004**: Memory overhead for registered validation services is under 5 MB when using default configuration
- **SC-005**: Developers can add custom proof systems or statement handlers without requiring more than 2 additional lines of code per extension
- **SC-006**: The integration works correctly in ASP.NET Core applications, console applications, and worker services without modification
- **SC-007**: Documentation for the integration pattern receives positive feedback with an average rating of 4.5/5 or higher from developer surveys
- **SC-008**: Zero runtime service resolution failures when following the documented integration pattern
- **SC-009**: Sample project demonstrates working validation flow within 50 lines of code (excluding boilerplate)
- **SC-010**: Developer support requests related to DI integration decrease by 75% compared to pre-integration patterns
