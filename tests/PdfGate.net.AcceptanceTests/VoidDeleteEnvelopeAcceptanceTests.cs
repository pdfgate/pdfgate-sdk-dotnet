using PdfGate.net;
using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for the void and delete envelope lifecycle against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class VoidDeleteEnvelopeAcceptanceTests
    : IClassFixture<EnvelopeSourceDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly EnvelopeSourceDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public VoidDeleteEnvelopeAcceptanceTests(PdfGateClientFixture clientFixture,
        EnvelopeSourceDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Creates a dedicated envelope, voids it with a reason, deletes it, and
    ///     verifies it is no longer retrievable. Voiding a created (never sent)
    ///     envelope sends no recipient emails.
    /// </summary>
    [Fact]
    public async Task VoidAndDeleteEnvelope_CompletesLifecycle()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        PdfGateEnvelope envelope = await client.CreateEnvelopeAsync(
            new CreateEnvelopeRequest
            {
                RequesterName = "SDK Acceptance Tests",
                Documents =
                [
                    new EnvelopeDocument
                    {
                        SourceDocumentId = source.Id,
                        Name = "Void Delete Agreement",
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
                ExpiresInDays = 10
            }, TestContext.Current.CancellationToken);

        Assert.NotNull(envelope.ExpiresAt);

        PdfGateEnvelope voided = await client.VoidEnvelopeAsync(
            new VoidEnvelopeRequest
            {
                Id = envelope.Id,
                Reason = "Acceptance test void"
            }, TestContext.Current.CancellationToken);

        Assert.Equal(EnvelopeStatus.Voided, voided.Status);
        Assert.NotNull(voided.VoidedAt);
        Assert.Equal("Acceptance test void", voided.VoidReason);

        await Assert.ThrowsAsync<PdfGateException>(async () =>
            await client.VoidEnvelopeAsync(new VoidEnvelopeRequest
            {
                Id = envelope.Id
            }, TestContext.Current.CancellationToken));

        await client.DeleteEnvelopeAsync(new DeleteEnvelopeRequest
        {
            Id = envelope.Id
        }, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<PdfGateException>(async () =>
            await client.GetEnvelopeAsync(new GetEnvelopeRequest
            {
                Id = envelope.Id
            }, TestContext.Current.CancellationToken));
    }
}
