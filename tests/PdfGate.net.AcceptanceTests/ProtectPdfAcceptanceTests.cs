using System.Text;

using PdfGate.net.Models;

using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for protect PDF operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class
    ProtectPdfAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with shared acceptance fixtures.
    /// </summary>
    public ProtectPdfAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Protects a PDF with all request values using AES-256 and verifies the resulting file requires one of the set
    ///     passwords.
    /// </summary>
    [Fact]
    public async Task
        ProtectPdfAsync_ByDocumentIdWithAllRequestValuesAes256_ReturnsEncryptedDocumentAndEncryptedFile()
    {
        PdfGate client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        var request = new ProtectPdfRequest
        {
            DocumentId = source.Id,
            Algorithm = ProtectPdfEncryptionAlgorithm.Aes256,
            UserPassword = Guid.NewGuid().ToString("N"),
            OwnerPassword = Guid.NewGuid().ToString("N"),
            DisablePrint = true,
            DisableCopy = true,
            DisableEditing = true,
            EncryptMetadata = true,
            PreSignedUrlExpiresIn = 3600,
            Metadata = new
            {
                author = "acceptance-test", source = "protect-aes256"
            }
        };

        PdfGateDocumentResponse protectedDocument =
            await client.ProtectPdfAsync(
                request,
                TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, protectedDocument.Status);
        Assert.Equal(DocumentType.Encrypted, protectedDocument.Type);
        Assert.Equal(source.Id, protectedDocument.DerivedFrom);

        Stream protectedFileStream = await client.GetFileAsync(
            new GetFileRequest { DocumentId = protectedDocument.Id },
            TestContext.Current.CancellationToken);
        var protectedFileBytes = await ReadAllBytesAsync(protectedFileStream,
            TestContext.Current.CancellationToken);

        AssertProtectedPdfRequiresPassword(protectedFileBytes,
            request.UserPassword!, request.OwnerPassword!);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream,
        CancellationToken cancellationToken)
    {
        await using var copy = new MemoryStream();
        await stream.CopyToAsync(copy, cancellationToken);
        return copy.ToArray();
    }

    private static void AssertProtectedPdfRequiresPassword(byte[] pdfBytes,
        string userPassword, string ownerPassword)
    {
        Assert.NotEmpty(pdfBytes);

        var pdfSignature = Encoding.ASCII.GetBytes("%PDF");
        Assert.True(pdfBytes.Length >= pdfSignature.Length);

        for (var i = 0; i < pdfSignature.Length; i++)
            Assert.Equal(pdfSignature[i], pdfBytes[i]);

        var pdfText = Encoding.Latin1.GetString(pdfBytes);
        Assert.Contains("/Encrypt", pdfText, StringComparison.Ordinal);

        Assert.Throws<PdfDocumentEncryptedException>(() =>
            PdfDocument.Open(pdfBytes));

        using PdfDocument openedWithUserPassword = PdfDocument.Open(pdfBytes,
            new ParsingOptions { Password = userPassword });
        Assert.True(openedWithUserPassword.NumberOfPages > 0);

        using PdfDocument openedWithOwnerPassword = PdfDocument.Open(pdfBytes,
            new ParsingOptions { Password = ownerPassword });
        Assert.True(openedWithOwnerPassword.NumberOfPages > 0);
    }
}
