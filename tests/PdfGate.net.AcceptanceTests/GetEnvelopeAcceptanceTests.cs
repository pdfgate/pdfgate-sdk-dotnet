using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for get envelope operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class GetEnvelopeAcceptanceTests
    : IClassFixture<EnvelopeSourceDocumentFixture>,
        IClassFixture<CreatedEnvelopeFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly EnvelopeSourceDocumentFixture _documentFixture;
    private readonly CreatedEnvelopeFixture _envelopeFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public GetEnvelopeAcceptanceTests(PdfGateClientFixture clientFixture,
        EnvelopeSourceDocumentFixture documentFixture,
        CreatedEnvelopeFixture envelopeFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
        _envelopeFixture = envelopeFixture;
    }

    /// <summary>
    ///     Calls the get envelope endpoint and verifies the created envelope state is returned.
    /// </summary>
    [Fact]
    public async Task GetEnvelopeAsync_ReturnsEnvelopeState()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateEnvelope envelope = await _envelopeFixture.GetEnvelopeOrSkipAsync(
            client, _documentFixture);

        PdfGateEnvelope response = await client.GetEnvelopeAsync(
            new GetEnvelopeRequest
            {
                Id = envelope.Id
            }, TestContext.Current.CancellationToken);

        Assert.Equal(envelope.Id, response.Id);
        Assert.NotNull(response.Status);
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
