using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

[Collection(AcceptanceTestCollection.Name)]
public class
    UploadFileAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public UploadFileAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Uploads a file
    /// </summary>
    [Fact]
    public async Task UploadFileAsync_UploadsFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse document =
            await _documentFixture.GetDocumentOrSkipAsync(client);
        var getFileRequest = new GetFileRequest { DocumentId = document.Id };
        Stream fileResponse = await client.GetFileAsync(getFileRequest,
            TestContext.Current.CancellationToken);

        var request = new UploadFileRequest { Content = fileResponse };
        PdfGateDocumentResponse uploadedResponse =
            await client.UploadFileAsync(request,
                TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, uploadedResponse.Status);
        Assert.Equal(DocumentType.Uploaded, uploadedResponse.Type);
    }

    /// <summary>
    ///     Uploads a file
    /// </summary>
    [Fact]
    public void UploadFile_UploadsFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse document =
            _documentFixture.GetDocumentOrSkip(client);
        var getFileRequest = new GetFileRequest { DocumentId = document.Id };
        Stream fileResponse = client.GetFile(getFileRequest,
            TestContext.Current.CancellationToken);

        var request = new UploadFileRequest { Content = fileResponse };
        PdfGateDocumentResponse uploadedResponse =
            client.UploadFile(request,
                TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, uploadedResponse.Status);
        Assert.Equal(DocumentType.Uploaded, uploadedResponse.Type);
    }

    /// <summary>
    ///     Uploads a file by URL
    /// </summary>
    [Fact(Skip = "The HTTP API is failing currently for this case")]
    public async Task UploadFileAsync_UploadsFileByUrl()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        var request = new UploadFileRequest
        {
            Url =
                new Uri(
                    "https://www.rd.usda.gov/sites/default/files/pdf-sample_0.pdf")
        };
        PdfGateDocumentResponse uploadedResponse =
            await client.UploadFileAsync(request,
                TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, uploadedResponse.Status);
        Assert.Equal(DocumentType.Uploaded, uploadedResponse.Type);
    }

    /// <summary>
    ///     Uploads a file by URL
    /// </summary>
    [Fact(Skip = "The HTTP API is failing currently for this case")]
    public void UploadFile_UploadsFileByUrl()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        var request = new UploadFileRequest
        {
            Url =
                new Uri(
                    "https://www.rd.usda.gov/sites/default/files/pdf-sample_0.pdf")
        };
        PdfGateDocumentResponse uploadedResponse =
            client.UploadFile(request,
                TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, uploadedResponse.Status);
        Assert.Equal(DocumentType.Uploaded, uploadedResponse.Type);
    }
}
