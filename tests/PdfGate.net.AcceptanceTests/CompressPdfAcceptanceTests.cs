using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for compress PDF operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class
    CompressPdfAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public CompressPdfAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Compresses a fixture document with all request fields and verifies completed compressed output and
    ///     reduced file size.
    /// </summary>
    [Fact]
    public async Task
        CompressPdfAsync_ByDocumentId_ReturnsCompressedCompletedAndSmallerFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        var sourceFileRequest = new GetFileRequest { DocumentId = source.Id };
        Stream sourceFileStream = await client.GetFileAsync(sourceFileRequest,
            TestContext.Current.CancellationToken);
        var sourceFileBytes = await ReadAllBytesAsync(sourceFileStream,
            TestContext.Current.CancellationToken);

        var request = new CompressPdfRequest
        {
            DocumentId = source.Id,
            Linearize = true,
            PreSignedUrlExpiresIn = 3600,
            Metadata = new
            {
                author = "acceptance-test", source = "compress"
            }
        };

        PdfGateDocumentResponse compressed = await client.CompressPdfAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, compressed.Status);
        Assert.Equal(DocumentType.Compressed, compressed.Type);
        Assert.Equal(source.Id, compressed.DerivedFrom);

        var compressedFileRequest =
            new GetFileRequest { DocumentId = compressed.Id };
        Stream compressedFileStream = await client.GetFileAsync(
            compressedFileRequest,
            TestContext.Current.CancellationToken);
        var compressedFileBytes = await ReadAllBytesAsync(
            compressedFileStream,
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(sourceFileBytes);
        Assert.NotEmpty(compressedFileBytes);
        Assert.True(compressedFileBytes.Length < sourceFileBytes.Length,
            $"Expected compressed file to be smaller. Source={sourceFileBytes.Length}, Compressed={compressedFileBytes.Length}");
    }

    /// <summary>
    ///     Compresses a fixture document with all request fields and verifies completed compressed output and
    ///     reduced file size.
    /// </summary>
    [Fact]
    public void CompressPdf_ByDocumentId_ReturnsCompressedCompletedAndSmallerFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            _documentFixture.GetDocumentOrSkip(client);

        var sourceFileRequest = new GetFileRequest { DocumentId = source.Id };
        Stream sourceFileStream = client.GetFile(sourceFileRequest,
            TestContext.Current.CancellationToken);
        var sourceFileBytes = ReadAllBytes(sourceFileStream);

        var request = new CompressPdfRequest
        {
            DocumentId = source.Id,
            Linearize = true,
            PreSignedUrlExpiresIn = 3600,
            Metadata = new
            {
                author = "acceptance-test", source = "compress"
            }
        };

        PdfGateDocumentResponse compressed = client.CompressPdf(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, compressed.Status);
        Assert.Equal(DocumentType.Compressed, compressed.Type);
        Assert.Equal(source.Id, compressed.DerivedFrom);

        var compressedFileRequest =
            new GetFileRequest { DocumentId = compressed.Id };
        Stream compressedFileStream = client.GetFile(
            compressedFileRequest,
            TestContext.Current.CancellationToken);
        var compressedFileBytes = ReadAllBytes(compressedFileStream);

        Assert.NotEmpty(sourceFileBytes);
        Assert.NotEmpty(compressedFileBytes);
        Assert.True(compressedFileBytes.Length < sourceFileBytes.Length,
            $"Expected compressed file to be smaller. Source={sourceFileBytes.Length}, Compressed={compressedFileBytes.Length}");
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream,
        CancellationToken cancellationToken)
    {
        await using var copy = new MemoryStream();
        await stream.CopyToAsync(copy, cancellationToken);
        return copy.ToArray();
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }
}
