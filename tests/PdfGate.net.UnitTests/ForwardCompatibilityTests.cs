using System.Net;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

/// <summary>
///     Verifies that unrecognized enum values from the API (e.g. newly added members) do not
///     fail response parsing — they are surfaced as null (or skipped, for the webhook list).
/// </summary>
public sealed class ForwardCompatibilityTests
{
    private static PdfGateClient ClientReturning(string body)
    {
        return new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((_, _) =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body)
                }));
    }

    [Fact]
    public void GetDocument_WithUnknownTypeAndStatus_ReturnsNullEnumsNotThrow()
    {
        const string body = """
            {"id":"doc_1","status":"brand_new_status","type":"brand_new_type","size":10,
             "createdAt":"2026-01-01T00:00:00Z"}
            """;
        using PdfGateClient client = ClientReturning(body);

        PdfGateDocumentResponse doc = client.GetDocument(
            new GetDocumentRequest { DocumentId = "doc_1" },
            TestContext.Current.CancellationToken);

        Assert.Equal("doc_1", doc.Id);
        Assert.Null(doc.Status);
        Assert.Null(doc.Type);
    }

    [Fact]
    public void GetDocument_WithKnownValues_StillParses()
    {
        const string body = """
            {"id":"doc_1","status":"completed","type":"document_fields_added","size":10,
             "createdAt":"2026-01-01T00:00:00Z"}
            """;
        using PdfGateClient client = ClientReturning(body);

        PdfGateDocumentResponse doc = client.GetDocument(
            new GetDocumentRequest { DocumentId = "doc_1" },
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, doc.Status);
        Assert.Equal(DocumentType.DocumentFieldsAdded, doc.Type);
    }

    [Fact]
    public void GetEnvelope_WithUnknownStatusesAndFieldType_ReturnsNullEnums()
    {
        const string body = """
            {"id":"env_1","status":"brand_new_env","createdAt":"2026-01-01T00:00:00Z",
             "documents":[{"sourceDocumentId":"d","status":"brand_new_doc",
             "recipients":[{"email":"a@b.com","status":"brand_new_recipient",
             "fields":[{"name":"f","type":"brand_new_field"}]}]}]}
            """;
        using PdfGateClient client = ClientReturning(body);

        PdfGateEnvelope env = client.GetEnvelope(
            new GetEnvelopeRequest { Id = "env_1" },
            TestContext.Current.CancellationToken);

        Assert.Equal("env_1", env.Id);
        Assert.Null(env.Status);
        Assert.Null(env.Documents[0].Status);
        Assert.Null(env.Documents[0].Recipients[0].Status);
        Assert.Null(env.Documents[0].Recipients[0].Fields[0].Type);
    }

    [Fact]
    public void GetWebhook_WithUnknownEventTypeAndStatus_SkipsUnknownAndNullsStatus()
    {
        const string body = """
            {"id":"wh_1","url":"https://x","status":"brand_new_status",
             "eventTypes":["envelope.completed","envelope.brand_new_event"],
             "createdAt":"2026-01-01T00:00:00Z"}
            """;
        using PdfGateClient client = ClientReturning(body);

        PdfGateWebhookResponse wh = client.GetWebhook(
            new GetWebhookRequest { Id = "wh_1" },
            TestContext.Current.CancellationToken);

        Assert.Equal("wh_1", wh.Id);
        Assert.Null(wh.Status);
        // Unknown event is skipped; the recognized one is kept.
        Assert.Equal(new[] { WebhookEventType.EnvelopeCompleted }, wh.EventTypes);
    }
}
