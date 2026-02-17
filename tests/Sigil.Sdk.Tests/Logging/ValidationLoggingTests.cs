using Microsoft.Extensions.Logging;
using Sigil.Sdk.Logging;
using Sigil.Sdk.Validation;
using Xunit;

namespace Sigil.Sdk.Tests.Logging;

public sealed class ValidationLoggingTests
{
    [Fact]
    public void LogValidationResult_NeverLogsProofBytesOrRawJson()
    {
        // Spec 002 (SC-005)
        var logger = new CapturingLogger();
        var fakeProofBytes = "VGhpcy1pcy1ub3QtcHJvb2ZCeXRlcw==";

        var result = new LicenseValidationResult(
            status: LicenseStatus.Malformed,
            envelopeVersion: "1.0",
            statementId: "stmt",
            proofSystem: "ps",
            claims: null,
            failure: new LicenseValidationFailure(
                LicenseFailureCode.SchemaValidationFailed,
                message: $"bad input {fakeProofBytes}",
                diagnosticException: null));

        ValidationLogging.LogValidationResult(logger, result);

        var combined = string.Join("\n", logger.Messages);
        Assert.DoesNotContain("proofBytes", combined);
        Assert.DoesNotContain(fakeProofBytes, combined);
        Assert.DoesNotContain("{\"", combined); // crude guard against raw JSON blobs
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NullScope();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
