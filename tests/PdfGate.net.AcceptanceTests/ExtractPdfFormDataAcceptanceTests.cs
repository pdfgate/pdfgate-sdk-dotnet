using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for extract PDF form data operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class
    ExtractPdfFormDataAcceptanceTests
{
    private readonly PdfGateClientFixture _fixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public ExtractPdfFormDataAcceptanceTests(PdfGateClientFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    ///     Extracts form data by existing document ID.
    /// </summary>
    [Fact]
    public async Task ExtractPdfFormDataAsync_ByDocumentId_ReturnsFieldValues()
    {
        PdfGate client = _fixture.GetClientOrSkip();
        PdfGateDocumentResponse source = await CreateDocumentWithFormAsync(
            client);

        var request =
            new ExtractPdfFormDataRequest { DocumentId = source.Id };

        JsonElement response = await client.ExtractPdfFormDataAsync(request,
            TestContext.Current.CancellationToken);

        Assert.Equal("John", response.GetProperty("first_name").GetString());
        Assert.Equal("Doe", response.GetProperty("last_name").GetString());
    }

    /// <summary>
    ///     Extracts form data by existing document ID.
    /// </summary>
    [Fact]
    public void ExtractPdfFormData_ByDocumentId_ReturnsFieldValues()
    {
        PdfGate client = _fixture.GetClientOrSkip();
        PdfGateDocumentResponse source = CreateDocumentWithForm(client);

        var request =
            new ExtractPdfFormDataRequest { DocumentId = source.Id };

        JsonElement response = client.ExtractPdfFormData(request,
            TestContext.Current.CancellationToken);

        Assert.Equal("John", response.GetProperty("first_name").GetString());
        Assert.Equal("Doe", response.GetProperty("last_name").GetString());
    }

    /// <summary>
    ///     Returns a PdfGateException with the HTTP API message when the API returns an error status.
    /// </summary>
    [Fact]
    public async Task
        ExtractPdfFormDataAsync_WhenApiReturnsError_ThrowsPdfGateExceptionWithApiMessage()
    {
        PdfGate client = _fixture.GetClientOrSkip();

        var request =
            new ExtractPdfFormDataRequest { DocumentId = "invalid-id" };

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.ExtractPdfFormDataAsync(request,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.ResponseBody));
        Assert.Contains(exception.ResponseBody!, exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    ///     Returns a PdfGateException with the HTTP API message when the API returns an error status.
    /// </summary>
    [Fact]
    public void ExtractPdfFormData_WhenApiReturnsError_ThrowsPdfGateExceptionWithApiMessage()
    {
        PdfGate client = _fixture.GetClientOrSkip();

        var request =
            new ExtractPdfFormDataRequest { DocumentId = "invalid-id" };

        var exception = Assert.Throws<PdfGateException>(() =>
            client.ExtractPdfFormData(request,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.ResponseBody));
        Assert.Contains(exception.ResponseBody!, exception.Message,
            StringComparison.Ordinal);
    }

    private static async Task<PdfGateDocumentResponse> CreateDocumentWithFormAsync(
        PdfGate client)
    {
        var generateRequest = new GeneratePdfRequest
        {
            Html =
                "<html><body><form><input type='text' name='first_name' value='John' /><input type='text' name='last_name' value='Doe' /></form></body></html>",
            EnableFormFields = true
        };

        return await client.GeneratePdfAsync(generateRequest,
            TestContext.Current.CancellationToken);
    }

    private static PdfGateDocumentResponse CreateDocumentWithForm(PdfGate client)
    {
        var generateRequest = new GeneratePdfRequest
        {
            Html =
                "<html><body><form><input type='text' name='first_name' value='John' /><input type='text' name='last_name' value='Doe' /></form></body></html>",
            EnableFormFields = true
        };

        return client.GeneratePdf(generateRequest,
            TestContext.Current.CancellationToken);
    }
}
