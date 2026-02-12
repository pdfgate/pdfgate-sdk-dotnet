using System.Text;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

[Collection(AcceptanceTestCollection.Name)]
public sealed class
    GetFileAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public GetFileAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Calls the GetFile endpoint and checks response is a PDF
    /// </summary>
    [Fact]
    public async Task GetFileAsync_ReturnsStreamWithFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse document =
            await _documentFixture.GetDocumentOrSkipAsync(client);
        var getFileRequest = new GetFileRequest { DocumentId = document.Id };

        Stream fileResponse = await client.GetFileAsync(getFileRequest,
            TestContext.Current.CancellationToken);

        var pdfSignature = Encoding.ASCII.GetBytes("%PDF");
        var buffer = new byte[pdfSignature.Length];
        var bytesRead = await fileResponse.ReadAsync(buffer, 0, buffer.Length,
            TestContext.Current.CancellationToken);

        Assert.Equal(bytesRead, pdfSignature.Length);

        for (var i = 0; i < pdfSignature.Length; i++)
            Assert.Equal(buffer[i], pdfSignature[i]);
    }
}
