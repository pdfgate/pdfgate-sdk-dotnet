using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class NewFeaturesTests
{
    private const string DocumentResponseBody = """
        {
          "id": "doc_new",
          "status": "completed",
          "type": "document_fields_added",
          "derivedFrom": "doc_123",
          "createdAt": "2024-02-13T15:56:12.607Z"
        }
        """;

    private static PdfGateClient CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> handler,
        Action<HttpRequestMessage>? capture = null)
    {
        return new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                capture?.Invoke(request);
                return handler(request);
            }));
    }

    [Fact]
    public void FlattenPdf_SendsFieldNames()
    {
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DocumentResponseBody)
            },
            request => body = request.Content!.ReadAsStringAsync()
                .GetAwaiter().GetResult());

        client.FlattenPdf(
            new FlattenPdfRequest
            {
                DocumentId = "doc_123",
                FieldNames = ["name", "email"]
            },
            TestContext.Current.CancellationToken);

        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement fieldNames = json.RootElement.GetProperty("fieldNames");
        Assert.Equal("name", fieldNames[0].GetString());
        Assert.Equal("email", fieldNames[1].GetString());
        Assert.True(json.RootElement.GetProperty("jsonResponse").GetBoolean());
    }

    [Fact]
    public void AddFormFields_SendsOverridesAndFieldsAndParsesResponse()
    {
        string? body = null;
        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DocumentResponseBody)
            },
            request =>
            {
                captured = request;
                body = request.Content!.ReadAsStringAsync()
                    .GetAwaiter().GetResult();
            });

        PdfGateDocumentResponse response = client.AddFormFields(
            new AddFormFieldsRequest
            {
                DocumentId = "doc_123",
                FieldOverrides = new Dictionary<string, FieldOverride>
                {
                    ["full_name"] = new() { Role = "signer", FontSize = 12 }
                },
                Fields =
                [
                    new ManualFormField
                    {
                        Name = "signed_on",
                        Type = DocumentFieldType.Date,
                        Page = 1,
                        X = 10,
                        Y = 20,
                        Width = 100,
                        Height = 24,
                        FontSize = 10
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("/forms/fields", captured!.RequestUri!.AbsolutePath);
        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement root = json.RootElement;
        Assert.Equal("doc_123", root.GetProperty("documentId").GetString());
        Assert.True(root.GetProperty("jsonResponse").GetBoolean());

        // Field-override keys must be preserved verbatim (not camelCased),
        // while override option keys are camelCased.
        JsonElement overrides = root.GetProperty("fieldOverrides");
        JsonElement fullName = overrides.GetProperty("full_name");
        Assert.Equal("signer", fullName.GetProperty("role").GetString());
        Assert.Equal(12, fullName.GetProperty("fontSize").GetInt32());

        JsonElement field = root.GetProperty("fields")[0];
        Assert.Equal("signed_on", field.GetProperty("name").GetString());
        Assert.Equal("date", field.GetProperty("type").GetString());
        Assert.Equal(10, field.GetProperty("fontSize").GetInt32());

        Assert.Equal(DocumentType.DocumentFieldsAdded, response.Type);
    }

    [Fact]
    public void DeleteDocument_SendsDeleteRequest()
    {
        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent),
            request => captured = request);

        client.DeleteDocument(
            new DeleteDocumentRequest { DocumentId = "doc_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/document/doc_123", captured.RequestUri!.AbsolutePath);
    }

    [Fact]
    public void CreateWebhook_SendsConfigAndParsesResponse()
    {
        const string responseBody = """
            {
              "id": "wh_123",
              "url": "https://example.com/hook",
              "eventTypes": ["envelope.completed", "envelope.sent"],
              "status": "active",
              "secret": "whsec_abc",
              "createdAt": "2024-02-13T15:56:12.607Z"
            }
            """;

        string? body = null;
        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            },
            request =>
            {
                captured = request;
                body = request.Content!.ReadAsStringAsync()
                    .GetAwaiter().GetResult();
            });

        PdfGateWebhookResponse response = client.CreateWebhook(
            new CreateWebhookRequest
            {
                Url = "https://example.com/hook",
                EventTypes =
                [
                    WebhookEventType.EnvelopeCompleted,
                    WebhookEventType.EnvelopeSent
                ],
                Description = "my hook"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("/webhook", captured!.RequestUri!.AbsolutePath);
        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement root = json.RootElement;
        Assert.Equal("https://example.com/hook",
            root.GetProperty("url").GetString());
        Assert.Equal("envelope.completed",
            root.GetProperty("eventTypes")[0].GetString());
        Assert.Equal("my hook", root.GetProperty("description").GetString());

        Assert.Equal("wh_123", response.Id);
        Assert.Equal(WebhookStatus.Active, response.Status);
        Assert.Equal(WebhookEventType.EnvelopeCompleted, response.EventTypes[0]);
        Assert.Equal("whsec_abc", response.Secret);
    }

    [Fact]
    public void GetWebhook_SendsGetRequestAndParsesResponse()
    {
        const string responseBody = """
            {
              "id": "wh_123",
              "url": "https://example.com/hook",
              "eventTypes": ["envelope.sent"],
              "status": "active",
              "createdAt": "2024-02-13T15:56:12.607Z"
            }
            """;

        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            },
            request => captured = request);

        PdfGateWebhookResponse response = client.GetWebhook(
            new GetWebhookRequest { Id = "wh_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/webhook/wh_123", captured.RequestUri!.AbsolutePath);
        Assert.Equal("wh_123", response.Id);
        Assert.Equal(WebhookStatus.Active, response.Status);
        Assert.Null(response.Secret);
    }

    [Fact]
    public void DeleteWebhook_SendsDeleteRequest()
    {
        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent),
            request => captured = request);

        client.DeleteWebhook(
            new DeleteWebhookRequest { Id = "wh_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/webhook/wh_123", captured.RequestUri!.AbsolutePath);
    }

    [Fact]
    public void CreateEnvelope_SerializesRecipientReminderFields()
    {
        const string responseBody = """
            { "id": "env_1", "status": "created", "documents": [] }
            """;
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            },
            request => body = request.Content!.ReadAsStringAsync()
                .GetAwaiter().GetResult());

        client.CreateEnvelope(
            new CreateEnvelopeRequest
            {
                RequesterName = "John",
                Documents =
                [
                    new EnvelopeDocument
                    {
                        SourceDocumentId = "doc_1",
                        Name = "Agreement",
                        Recipients =
                        [
                            new EnvelopeRecipient
                            {
                                Email = "a@example.com",
                                Name = "Anna",
                                ReminderIntervalDays = 2,
                                ReminderAttempts = 3
                            }
                        ]
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement recipient = json.RootElement.GetProperty("documents")[0]
            .GetProperty("recipients")[0];
        Assert.Equal(2, recipient.GetProperty("reminderIntervalDays").GetInt32());
        Assert.Equal(3, recipient.GetProperty("reminderAttempts").GetInt32());
    }

    [Fact]
    public void GetEnvelope_ParsesRecipientLinksAndFieldTimezoneFields()
    {
        const string responseBody = """
            {
              "id": "env_1",
              "status": "in_progress",
              "documents": [
                {
                  "sourceDocumentId": "doc_1",
                  "status": "pending",
                  "recipients": [
                    {
                      "email": "a@example.com",
                      "status": "pending",
                      "signingLink": "https://sign.example/abc",
                      "previewLink": "https://preview.example/abc",
                      "fields": [
                        {
                          "name": "signature-date",
                          "type": "datetime",
                          "timezone": "UTC",
                          "source": "user",
                          "userValue": "2026-01-01T10:00:00",
                          "userTimezone": "Europe/Athens"
                        }
                      ]
                    }
                  ]
                }
              ],
              "createdAt": "2024-02-13T15:56:12.607Z"
            }
            """;

        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            });

        PdfGateEnvelope envelope = client.GetEnvelope(
            new GetEnvelopeRequest { Id = "env_1" },
            TestContext.Current.CancellationToken);

        EnvelopeRecipientResponse recipient =
            envelope.Documents[0].Recipients[0];
        Assert.Equal("https://sign.example/abc", recipient.SigningLink);
        Assert.Equal("https://preview.example/abc", recipient.PreviewLink);

        EnvelopeFieldResponse field = recipient.Fields[0];
        Assert.Equal("UTC", field.Timezone);
        Assert.Equal("user", field.Source);
        Assert.Equal("2026-01-01T10:00:00", field.UserValue);
        Assert.Equal("Europe/Athens", field.UserTimezone);
    }
}
