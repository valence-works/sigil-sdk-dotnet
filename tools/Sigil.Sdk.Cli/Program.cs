using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Sigil.Sdk.DependencyInjection;
using Sigil.Sdk.Validation;

return await SigilCli.RunAsync(args);

internal static class SigilCli
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length is 0 || IsHelp(args[0]))
        {
            WriteUsage();
            return args.Length is 0 ? 2 : 0;
        }

        if (!string.Equals(args[0], "verify-envelope", StringComparison.Ordinal))
        {
            Console.Error.WriteLine($"Unknown command: {args[0]}");
            WriteUsage();
            return 2;
        }

        if (args.Length != 2)
        {
            Console.Error.WriteLine("verify-envelope requires exactly one envelope JSON file path.");
            WriteUsage();
            return 2;
        }

        return await VerifyEnvelopeAsync(args[1]).ConfigureAwait(false);
    }

    private static async Task<int> VerifyEnvelopeAsync(string envelopePath)
    {
        if (!File.Exists(envelopePath))
        {
            Console.Error.WriteLine($"Envelope file not found: {envelopePath}");
            return 2;
        }

        try
        {
            await using var stream = File.OpenRead(envelopePath);
            using var provider = new ServiceCollection()
                .AddSigilValidation()
                .BuildServiceProvider();

            var validator = provider.GetRequiredService<ILicenseValidator>();
            var result = await validator.ValidateAsync(stream).ConfigureAwait(false);

            Console.WriteLine(JsonSerializer.Serialize(new VerificationOutput(
                result.IsValid,
                result.Status.ToString(),
                result.EnvelopeVersion,
                result.StatementId,
                result.ProofSystem,
                result.Failure?.Code.ToString(),
                result.Failure?.Message), JsonOptions));

            return result.IsValid ? 0 : 1;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not read envelope file: {ex.Message}");
            return 2;
        }
    }

    private static bool IsHelp(string value)
    {
        return string.Equals(value, "-h", StringComparison.Ordinal)
            || string.Equals(value, "--help", StringComparison.Ordinal)
            || string.Equals(value, "help", StringComparison.Ordinal);
    }

    private static void WriteUsage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  dotnet run --project tools/Sigil.Sdk.Cli -- verify-envelope <envelope.json>");
    }

    private sealed record VerificationOutput(
        bool IsValid,
        string Status,
        string? EnvelopeVersion,
        string? StatementId,
        string? ProofSystem,
        string? FailureCode,
        string? FailureMessage);
}
