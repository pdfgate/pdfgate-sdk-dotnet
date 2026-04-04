using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for create envelope operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class CreateEnvelopeAcceptanceTests
    : IClassFixture<EnvelopeSourceDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly EnvelopeSourceDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public CreateEnvelopeAcceptanceTests(PdfGateClientFixture clientFixture,
        EnvelopeSourceDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Calls the create envelope endpoint and verifies the created response.
    /// </summary>
    [Fact]
    public async Task CreateEnvelopeAsync_ReturnsCreatedEnvelope()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        var request = new CreateEnvelopeRequest
        {
            RequesterName = "SDK Acceptance Tests",
            Documents =
            [
                new EnvelopeDocument
                {
                    SourceDocumentId = source.Id,
                    Name = "Agreement",
                    Recipients =
                    [
                        new EnvelopeRecipient
                        {
                            Email = "anna@example.com",
                            Name = "Anna Smith"
                        }
                    ]
                }
            ],
            Metadata = new
            {
                customerId = "cus_123",
                department = "sales"
            }
        };

        PdfGateEnvelope response = await client.CreateEnvelopeAsync(request,
            TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(response.Id));
        Assert.Equal(EnvelopeStatus.Created, response.Status);
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
