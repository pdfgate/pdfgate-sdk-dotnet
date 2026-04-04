using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for send envelope operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class SendEnvelopeAcceptanceTests
    : IClassFixture<EnvelopeSourceDocumentFixture>,
        IClassFixture<CreatedEnvelopeFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly EnvelopeSourceDocumentFixture _documentFixture;
    private readonly CreatedEnvelopeFixture _envelopeFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public SendEnvelopeAcceptanceTests(PdfGateClientFixture clientFixture,
        EnvelopeSourceDocumentFixture documentFixture,
        CreatedEnvelopeFixture envelopeFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
        _envelopeFixture = envelopeFixture;
    }

    /// <summary>
    ///     Calls the send envelope endpoint and verifies the envelope is in progress.
    /// </summary>
    [Fact]
    public async Task SendEnvelopeAsync_ReturnsInProgressEnvelope()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateEnvelope envelope = await _envelopeFixture.GetEnvelopeOrSkipAsync(
            client, _documentFixture);

        PdfGateEnvelope response = await client.SendEnvelopeAsync(
            new SendEnvelopeRequest
            {
                Id = envelope.Id
            }, TestContext.Current.CancellationToken);

        Assert.Equal(envelope.Id, response.Id);
        Assert.Equal(EnvelopeStatus.InProgress, response.Status);
        Assert.NotNull(response.CreatedAt);
        Assert.NotEmpty(response.Documents);
        Assert.True(response.Metadata.HasValue);
        var metadata = response.Metadata.GetValueOrDefault();
        Assert.Equal("cus_123",
            metadata.GetProperty("customerId").GetString());
        Assert.Equal("sales",
            metadata.GetProperty("department").GetString());
    }
}
