using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for the newly added features against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class NewFeaturesAcceptanceTests
{
    private const string FormHtml =
        "<html><body><form>"
        + "<input type='text' name='first_name' value='John' />"
        + "<input type='text' name='last_name' value='Doe' />"
        + "</form></body></html>";

    private readonly PdfGateClientFixture _fixture;

    public NewFeaturesAcceptanceTests(PdfGateClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FlattenPdfAsync_WithFieldNames_ReturnsFlattened()
    {
        PdfGateClient client = _fixture.GetClientOrSkip();

        PdfGateDocumentResponse source = await client.GeneratePdfAsync(
            new GeneratePdfRequest { Html = FormHtml, EnableFormFields = true },
            TestContext.Current.CancellationToken);

        PdfGateDocumentResponse flattened = await client.FlattenPdfAsync(
            new FlattenPdfRequest
            {
                DocumentId = source.Id,
                FieldNames = ["first_name"]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, flattened.Status);
        Assert.Equal(DocumentType.Flattened, flattened.Type);
    }

    [Fact]
    public async Task AddFormFieldsAsync_WithManualField_ReturnsDocument()
    {
        PdfGateClient client = _fixture.GetClientOrSkip();

        PdfGateDocumentResponse source = await client.GeneratePdfAsync(
            new GeneratePdfRequest
            {
                Html = "<html><body><h1>Add form fields</h1></body></html>"
            },
            TestContext.Current.CancellationToken);

        PdfGateDocumentResponse result = await client.AddFormFieldsAsync(
            new AddFormFieldsRequest
            {
                DocumentId = source.Id,
                Fields =
                [
                    new ManualFormField
                    {
                        Name = "full_name",
                        Type = DocumentFieldType.Text,
                        Page = 1,
                        X = 50,
                        Y = 500,
                        Width = 200,
                        Height = 24
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.NotEqual(source.Id, result.Id);
        Assert.Equal(DocumentStatus.Completed, result.Status);
        Assert.Equal(DocumentType.DocumentFieldsAdded, result.Type);
    }

    [Fact]
    public async Task DeleteDocumentAsync_RemovesDocument()
    {
        PdfGateClient client = _fixture.GetClientOrSkip();

        PdfGateDocumentResponse doc = await client.GeneratePdfAsync(
            new GeneratePdfRequest
            {
                Html = "<html><body><h1>Delete me</h1></body></html>"
            },
            TestContext.Current.CancellationToken);

        await client.DeleteDocumentAsync(
            new DeleteDocumentRequest { DocumentId = doc.Id },
            TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<PdfGateException>(() =>
            client.GetDocumentAsync(
                new GetDocumentRequest { DocumentId = doc.Id },
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WebhookAsync_CreateGetDelete_Lifecycle()
    {
        PdfGateClient client = _fixture.GetClientOrSkip();

        var url = $"https://example.com/pdfgate-hook-{Guid.NewGuid():N}";

        PdfGateWebhookResponse created = await client.CreateWebhookAsync(
            new CreateWebhookRequest
            {
                Url = url,
                EventTypes =
                [
                    WebhookEventType.EnvelopeCompleted,
                    WebhookEventType.EnvelopeSent
                ],
                Description = ".NET SDK acceptance test"
            },
            TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(created.Id));
        Assert.Equal(url, created.Url);
        Assert.Equal(WebhookStatus.Active, created.Status);
        Assert.False(string.IsNullOrEmpty(created.Secret));

        try
        {
            PdfGateWebhookResponse fetched = await client.GetWebhookAsync(
                new GetWebhookRequest { Id = created.Id },
                TestContext.Current.CancellationToken);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Null(fetched.Secret);
        }
        finally
        {
            await client.DeleteWebhookAsync(
                new DeleteWebhookRequest { Id = created.Id },
                TestContext.Current.CancellationToken);
        }

        await Assert.ThrowsAsync<PdfGateException>(() =>
            client.GetWebhookAsync(
                new GetWebhookRequest { Id = created.Id },
                TestContext.Current.CancellationToken));
    }
}
