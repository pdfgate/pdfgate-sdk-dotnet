using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Shared fixture that creates an envelope suitable for send envelope acceptance tests.
/// </summary>
public sealed class CreatedEnvelopeFixture
{
    private PdfGateEnvelope? _envelope;

    /// <summary>
    ///     Returns a created envelope or skips the test when no client is available.
    /// </summary>
    public async Task<PdfGateEnvelope> GetEnvelopeOrSkipAsync(PdfGateClient? client,
        EnvelopeSourceDocumentFixture documentFixture)
    {
        if (_envelope != null)
            return _envelope;

        if (client is null)
            Assert.Skip("No client to get an example envelope.");

        PdfGateDocumentResponse source =
            await documentFixture.GetDocumentOrSkipAsync(client);

        _envelope = await client.CreateEnvelopeAsync(new CreateEnvelopeRequest
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
        }, TestContext.Current.CancellationToken);

        return _envelope;
    }
}
