// Spec 003 (T016, SC-001): Controller demonstrating ISigilValidator injection and usage

using Microsoft.AspNetCore.Mvc;
using Sigil.Sdk.Validation;

namespace MinimalDiSample.Controllers;

/// <summary>
/// Demonstrates minimal usage of ISigilValidator via dependency injection.
/// Spec 003 (SC-001, SC-006): Validates proof envelopes in ASP.NET Core scenario
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly ILicenseValidator validator;

    /// <summary>
    /// Inject ISigilValidator from DI container.
    /// The validator is automatically registered by AddSigilValidation()
    /// </summary>
    public ValidationController(ILicenseValidator validator)
    {
        this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Validate a proof envelope.
    /// 
    /// Sample request body:
    /// {
    ///   "version": "1.0",
    ///   "proof": { "proofSystem": "test", "publicInputs": {}, "proofBytes": "..." },
    ///   "statements": []
    /// }
    /// </summary>
    /// <param name="envelopeJson">The proof envelope JSON to validate (Spec 001)</param>
    /// <returns>Validation result with success/failure details</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(LicenseValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LicenseValidationResult>> ValidateProofEnvelope(
        [FromBody] string? envelopeJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(envelopeJson))
        {
            return BadRequest("Envelope cannot be null or empty");
        }

        try
        {
            // Spec 003 (FR-002): Validator is injected and ready to use
            // This demonstrates the minimal setup works end-to-end
            var result = await validator.ValidateAsync(envelopeJson, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint demonstrating that validator is initialized.
    /// Verifies DI container has successfully wired all dependencies.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> Health()
    {
        return Ok(new { status = "healthy", validator_type = validator.GetType().Name });
    }
}
