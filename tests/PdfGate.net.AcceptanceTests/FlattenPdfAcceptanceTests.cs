using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for flatten PDF operations against the live API.
/// </summary>
public sealed class
    FlattenPdfAcceptanceTests : IClassFixture<PdfGateClientFixture>
{
    private readonly PdfGateClientFixture _fixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public FlattenPdfAcceptanceTests(PdfGateClientFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    ///     Calls the flatten PDF method by document ID and verifies completion status.
    /// </summary>
    [Fact]
    public async Task FlattenPdfAsync_ByDocumentId_ReturnsCompletedStatus()
    {
        PdfGate client = _fixture.GetClientOrSkip();

        var generateRequest = new GeneratePdfRequest
        {
            Html =
                "<html><body><h1>Flatten Acceptance Test</h1><input type='text' value='hello' /></body></html>",
            EnableFormFields = true
        };

        PdfGateDocumentResponse source = await client.GeneratePdfAsync(
            generateRequest,
            TestContext.Current.CancellationToken);

        var flattenRequest = new FlattenPdfRequest { DocumentId = source.Id };

        PdfGateDocumentResponse flattened = await client.FlattenPdfAsync(
            flattenRequest,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, flattened.Status);
        Assert.Equal(DocumentType.Flattened, flattened.Type);
    }

    /// <summary>
    ///     Returns a PdfGateException with the HTTP API message when the API returns an error status.
    /// </summary>
    [Fact]
    public async Task
        FlattenPdfAsync_WhenApiReturnsError_ThrowsPdfGateExceptionWithApiMessage()
    {
        PdfGate client = _fixture.GetClientOrSkip();

        var request = new FlattenPdfRequest();

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.FlattenPdfAsync(request,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.ResponseBody));
        Assert.Contains(exception.ResponseBody!, exception.Message,
            StringComparison.Ordinal);
    }
}
