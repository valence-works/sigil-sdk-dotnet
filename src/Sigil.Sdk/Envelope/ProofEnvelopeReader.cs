// Spec 002 (FR-006): Parse and extract routing identifiers early.

using System.Text;
using System.Text.Json;

namespace Sigil.Sdk.Envelope;

public static class ProofEnvelopeReader
{
    public static ProofEnvelopeReadResult ReadFromString(string envelopeJson)
    {
        if (envelopeJson is null)
        {
            throw new ArgumentNullException(nameof(envelopeJson));
        }

        var document = JsonDocument.Parse(envelopeJson);
        return Extract(document);
    }

    public static async Task<ProofEnvelopeReadResult> ReadFromStreamAsync(Stream envelopeStream, CancellationToken cancellationToken = default)
    {
        if (envelopeStream is null)
        {
            throw new ArgumentNullException(nameof(envelopeStream));
        }

        using var ms = new MemoryStream();
        await envelopeStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(ms.ToArray());
        return ReadFromString(json);
    }

    private static ProofEnvelopeReadResult Extract(JsonDocument document)
    {
        var root = document.RootElement;
        string? envelopeVersion = TryGetStringProperty(root, "envelopeVersion");
        string? proofSystem = TryGetStringProperty(root, "proofSystem");
        string? statementId = TryGetStringProperty(root, "statementId");

        return new ProofEnvelopeReadResult(document, envelopeVersion, proofSystem, statementId);
    }

    private static string? TryGetStringProperty(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!root.TryGetProperty(name, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }
}

public sealed class ProofEnvelopeReadResult
{
    public ProofEnvelopeReadResult(JsonDocument document, string? envelopeVersion, string? proofSystem, string? statementId)
    {
        Document = document;
        EnvelopeVersion = envelopeVersion;
        ProofSystem = proofSystem;
        StatementId = statementId;
    }

    public JsonDocument Document { get; }

    public JsonElement Root => Document.RootElement;

    public string? EnvelopeVersion { get; }

    public string? ProofSystem { get; }

    public string? StatementId { get; }
}
