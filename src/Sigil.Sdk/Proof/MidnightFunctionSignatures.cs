using System;

namespace Sigil.Sdk.Proof;

/// <summary>
/// Function signature bindings for Midnight proof verification WASM module.
/// Reverse-engineered from @midnight-ntwrk/proof-verification TypeScript definitions.
/// 
/// Phase 2.5 PoC: T017 - Validates that WASM exports essential proof verification functions.
/// 
/// Module interface (Midnight):
/// - Input: (proofData: Uint8Array, publicInput: Uint8Array) → (verified: u32, errorCode: u32)
/// - Returns: 0 = verified, 1 = invalid proof, 2+ = error codes (see ProofVerificationError enum)
/// 
/// Design: Keep bindings thin; actual VerifyAsync logic in MidnightZkV1ProofSystemVerifier.
/// </summary>
internal static class MidnightFunctionSignatures
{
    /// <summary>
    /// Main proof verification function from Midnight WASM module.
    /// 
    /// Signature: verify_proof(proof_ptr: i32, proof_len: i32, context_ptr: i32, context_len: i32) -> i32
    /// 
    /// Returns:
    /// - 0: Proof verified successfully
    /// - 1: Invalid proof (signature/curve check failed)
    /// - 2: Invalid context (malformed public input)
    /// - 3+: Internal error codes (see MidnightWasmError enum)
    /// </summary>
    public const string VerifyProofFunctionName = "verify_proof";

    /// <summary>
    /// Retrieves the last error message from Midnight WASM module (if verification failed).
    /// Used for debugging and diagnostics (Phase 4).
    /// 
    /// Signature: get_last_error() -> i32  (pointer to error string in linear memory)
    /// 
    /// Returns pointer to null-terminated string in WASM linear memory.
    /// Caller must read memory and free if necessary.
    /// </summary>
    public const string GetLastErrorFunctionName = "get_last_error";

    /// <summary>
    /// Returns Midnight WASM module version as semantic version string.
    /// Used for logging and telemetry (Phase 4).
    /// 
    /// Signature: get_version() -> i32  (pointer to version string in linear memory)
    /// 
    /// Returns pointer to null-terminated string like "1.0.0" in WASM linear memory.
    /// </summary>
    public const string GetVersionFunctionName = "get_version";
}

/// <summary>
/// Error codes returned by Midnight WASM verify_proof function.
/// Maps Midnight Zk errors to ProofVerificationOutcome enum.
/// </summary>
internal enum MidnightWasmError : uint
{
    /// <summary>Proof signature valid for given public input.</summary>
    Verified = 0,

    /// <summary>Proof signature check failed or invalid curve point.</summary>
    InvalidProof = 1,

    /// <summary>Malformed public input or context.</summary>
    InvalidContext = 2,

    /// <summary>Memory allocation or internal bounds check failed.</summary>
    MemoryError = 3,

    /// <summary>Serialization/deserialization failed.</summary>
    SerializationError = 4,

    /// <summary>Cryptographic primitive failed (curve, field arithmetic).</summary>
    CryptoError = 5,

    /// <summary>Unexpected/unrecoverable internal error.</summary>
    InternalError = 999,
}

/// <summary>
/// Function parameter types for Midnight WASM interface.
/// Used to validate and construct function calls with correct signatures.
/// </summary>
internal static class MidnightFunctionParameters
{
    /// <summary>
    /// Proof data pointer (linear memory offset) and length.
    /// Typically: 8 bytes (i64 pair) or 16 bytes (i32,i32 pair).
    /// </summary>
    public const int ProofMemoryPtrSize = 4;      // i32 pointer
    public const int ProofMemoryLenSize = 4;      // i32 length

    /// <summary>
    /// Public input (context) pointer and length for verification.
    /// Same as proof: typically i32 pointers.
    /// </summary>
    public const int ContextMemoryPtrSize = 4;    // i32 pointer
    public const int ContextMemoryLenSize = 4;    // i32 length

    /// <summary>
    /// Memory page size in WASM (64 KB per page).
    /// Used to calculate required pages for proof/context buffers.
    /// </summary>
    public const int WasmMemoryPageSize = 65536;  // 64 KB
}
