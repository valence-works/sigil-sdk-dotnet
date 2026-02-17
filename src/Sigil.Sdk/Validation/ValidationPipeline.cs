// Spec 002 (FR-013a): First failing stage wins, using a strict pipeline order.

namespace Sigil.Sdk.Validation;

public static class ValidationPipeline
{
    public const string ReadAndParse = "read.parse";
    public const string EarlyExtract = "extract.identifiers";
    public const string Schema = "schema";
    public const string RegistryResolution = "registry";
    public const string ProofVerification = "crypto";
    public const string StatementValidation = "semantic";
    public const string ExpiryEvaluation = "expiry";
    public const string InternalError = "error";
}
