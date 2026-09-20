using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class SigningOrderTests
{
    private const string EmptyEnvelopeResponseBody = """
        { "id": "env_1", "status": "created", "documents": [] }
        """;

    private static PdfGateClient CreateClient(string responseBody,
        Action<string>? captureBody = null)
    {
        return new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                captureBody?.Invoke(request.Content!.ReadAsStringAsync()
                    .GetAwaiter().GetResult());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                };
            }));
    }

    [Fact]
    public void CreateEnvelope_SerializesSigningOrder()
    {
        string? body = null;
        using PdfGateClient client =
            CreateClient(EmptyEnvelopeResponseBody, b => body = b);

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
                                SigningOrder = 1
                            },
                            new EnvelopeRecipient
                            {
                                Email = "b@example.com",
                                Name = "Ben",
                                SigningOrder = 2
                            }
                        ]
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement recipients = json.RootElement.GetProperty("documents")[0]
            .GetProperty("recipients");
        Assert.Equal(1, recipients[0].GetProperty("signingOrder").GetInt32());
        Assert.Equal(2, recipients[1].GetProperty("signingOrder").GetInt32());
    }

    [Fact]
    public void CreateEnvelope_OmitsSigningOrderWhenUnset()
    {
        string? body = null;
        using PdfGateClient client =
            CreateClient(EmptyEnvelopeResponseBody, b => body = b);

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
                                Name = "Anna"
                            }
                        ]
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement recipient = json.RootElement.GetProperty("documents")[0]
            .GetProperty("recipients")[0];
        Assert.False(recipient.TryGetProperty("signingOrder", out _));
    }

    [Fact]
    public void GetEnvelope_ParsesSigningOrderAndActivatedAt()
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
                      "signingOrder": 2,
                      "activatedAt": "2026-02-13T15:56:12.607Z",
                      "fields": []
                    }
                  ]
                }
              ],
              "createdAt": "2026-02-13T15:56:12.607Z"
            }
            """;

        using PdfGateClient client = CreateClient(responseBody);

        PdfGateEnvelope envelope = client.GetEnvelope(
            new GetEnvelopeRequest { Id = "env_1" },
            TestContext.Current.CancellationToken);

        EnvelopeRecipientResponse recipient =
            envelope.Documents[0].Recipients[0];
        Assert.Equal(2, recipient.SigningOrder);
        Assert.Equal(
            new DateTimeOffset(2026, 2, 13, 15, 56, 12, 607, TimeSpan.Zero),
            recipient.ActivatedAt);
    }

    [Fact]
    public void GetEnvelope_NullsSigningOrderAndActivatedAtWhenAbsent()
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
                      "fields": []
                    }
                  ]
                }
              ],
              "createdAt": "2026-02-13T15:56:12.607Z"
            }
            """;

        using PdfGateClient client = CreateClient(responseBody);

        PdfGateEnvelope envelope = client.GetEnvelope(
            new GetEnvelopeRequest { Id = "env_1" },
            TestContext.Current.CancellationToken);

        EnvelopeRecipientResponse recipient =
            envelope.Documents[0].Recipients[0];
        Assert.Null(recipient.SigningOrder);
        Assert.Null(recipient.ActivatedAt);
    }

    [Fact]
    public void WebhookEventType_EnvelopeRecipientActivated_RoundTrips()
    {
        var serialized = JsonSerializer.Serialize(
            WebhookEventType.EnvelopeRecipientActivated);
        Assert.Equal("\"envelope.recipient.activated\"", serialized);

        WebhookEventType deserialized =
            JsonSerializer.Deserialize<WebhookEventType>(serialized);
        Assert.Equal(WebhookEventType.EnvelopeRecipientActivated,
            deserialized);
    }
}
