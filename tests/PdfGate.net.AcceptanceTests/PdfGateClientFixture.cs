using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Shared fixture for acceptance tests that require a configured API client.
/// </summary>
public sealed class PdfGateClientFixture : IDisposable
{
    private const string MissingApiKeyMessage =
        "Set PDFGATE_API_KEY to run acceptance tests.";

    private readonly PdfGate? _client;

    /// <summary>
    ///     Initializes the client when a test API key is available.
    /// </summary>
    public PdfGateClientFixture()
    {
        var apiKey = Environment.GetEnvironmentVariable("PDFGATE_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
            _client = new PdfGate(apiKey);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client?.Dispose();
    }

    /// <summary>
    ///     Returns the configured client or skips the test when no API key is set.
    /// </summary>
    public PdfGate GetClientOrSkip()
    {
        if (_client is null)
            Assert.Skip(MissingApiKeyMessage);

        return _client;
    }
}
