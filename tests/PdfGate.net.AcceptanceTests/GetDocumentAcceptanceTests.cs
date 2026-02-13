using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

[Collection(AcceptanceTestCollection.Name)]
public sealed class
    GetDocumentAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public GetDocumentAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Calls the GetDocument endpoint and verifies the fixture document is returned as completed.
    /// </summary>
    [Fact]
    public async Task GetDocumentAsync_ReturnsFixtureDocumentWithCompletedStatus()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        var request = new GetDocumentRequest
        {
            DocumentId = source.Id
        };

        PdfGateDocumentResponse response = await client.GetDocumentAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(source.Id, response.Id);
        Assert.Equal(DocumentStatus.Completed, response.Status);
    }
}
