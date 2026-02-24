using System.Text.Json.Serialization;

namespace Sigil.Sdk.Tests.Validation.Conformance;

/// <summary>
/// Represents a single Midnight conformance test vector.
/// Used to drive vector-based validation testing per FR-014, FR-015.
/// </summary>
public class MidnightConformanceVector
{
    [JsonPropertyName("testId")]
    public string TestId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("statementId")]
    public string StatementId { get; set; } = string.Empty;

    [JsonPropertyName("proofSystem")]
    public string ProofSystem { get; set; } = string.Empty;

    [JsonPropertyName("proofBytes")]
    public string ProofBytes { get; set; } = string.Empty;

    [JsonPropertyName("expectedOutcome")]
    public string ExpectedOutcome { get; set; } = string.Empty;

    [JsonPropertyName("expectedFailureCode")]
    public string? ExpectedFailureCode { get; set; }

    [JsonPropertyName("context")]
    public Dictionary<string, object>? Context { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("checksum")]
    public string? Checksum { get; set; }

    [JsonPropertyName("simulationStrategy")]
    public string? SimulationStrategy { get; set; }
}

/// <summary>
/// Utility class for loading and managing Midnight conformance test vectors.
/// Implements conformance vector loading per FR-014, FR-015.
/// </summary>
public static class MidnightConformanceVectors
{
    private static readonly string VectorsDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "Validation", "Conformance", "Vectors");

    /// <summary>
    /// Loads all conformance vectors from the Vectors directory.
    /// </summary>
    /// <returns>Collection of conformance vectors.</returns>
    public static IEnumerable<MidnightConformanceVector> LoadAll()
    {
        var vectors = new List<MidnightConformanceVector>();

        if (!Directory.Exists(VectorsDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Conformance vectors directory not found: {VectorsDirectory}");
        }

        var jsonFiles = Directory.GetFiles(VectorsDirectory, "*.json");

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = System.IO.File.ReadAllText(filePath);
                var vector = System.Text.Json.JsonSerializer.Deserialize<MidnightConformanceVector>(json);
                
                if (vector != null)
                {
                    vectors.Add(vector);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load conformance vector from {filePath}: {ex.Message}", ex);
            }
        }

        return vectors;
    }

    /// <summary>
    /// Loads a single conformance vector by filename.
    /// </summary>
    public static MidnightConformanceVector? Load(string filename)
    {
        var filePath = Path.Combine(VectorsDirectory, filename);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = System.IO.File.ReadAllText(filePath);
            return System.Text.Json.JsonSerializer.Deserialize<MidnightConformanceVector>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load conformance vector from {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all vectors with a specific expected outcome.
    /// </summary>
    public static IEnumerable<MidnightConformanceVector> GetByOutcome(string outcome)
    {
        return LoadAll().Where(v => v.ExpectedOutcome == outcome);
    }

    /// <summary>
    /// Gets the first vector with a specific expected outcome.
    /// </summary>
    public static MidnightConformanceVector? GetFirstByOutcome(string outcome)
    {
        return GetByOutcome(outcome).FirstOrDefault();
    }

    /// <summary>
    /// Gets all valid proof vectors (expectedOutcome = "Verified").
    /// </summary>
    public static IEnumerable<MidnightConformanceVector> GetValidVectors()
    {
        return GetByOutcome("Verified");
    }

    /// <summary>
    /// Gets all invalid proof vectors (expectedOutcome = "Invalid").
    /// </summary>
    public static IEnumerable<MidnightConformanceVector> GetInvalidVectors()
    {
        return GetByOutcome("Invalid");
    }

    /// <summary>
    /// Gets all error vectors (expectedOutcome = "Error").
    /// </summary>
    public static IEnumerable<MidnightConformanceVector> GetErrorVectors()
    {
        return GetByOutcome("Error");
    }

    /// <summary>
    /// Validates that all required conformance vectors are present.
    /// Per FR-015: minimum 3 vectors (valid, invalid, error).
    /// </summary>
    public static (bool IsValid, List<string> MissingVectors) ValidateRequiredVectors()
    {
        var vectors = LoadAll();
        var missing = new List<string>();

        if (!vectors.Any(v => v.ExpectedOutcome == "Verified"))
        {
            missing.Add("Known-valid Midnight proof vector");
        }

        if (!vectors.Any(v => v.ExpectedOutcome == "Invalid"))
        {
            missing.Add("Known-invalid Midnight proof vector");
        }

        if (!vectors.Any(v => v.ExpectedOutcome == "Error"))
        {
            missing.Add("Internal-error simulation vector");
        }

        return (missing.Count == 0, missing);
    }
}
