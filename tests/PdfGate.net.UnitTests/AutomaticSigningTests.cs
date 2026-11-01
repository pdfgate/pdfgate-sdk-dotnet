using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class AutomaticSigningTests
{
    private const string EmptyEnvelopeResponseBody = """
        { "id": "env_1", "status": "created", "documents": [] }
        """;

    private static PdfGateClient CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond,
        Action<HttpRequestMessage, string?>? capture = null)
    {
        return new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                var body = request.Content?.ReadAsStringAsync()
                    .GetAwaiter().GetResult();
                capture?.Invoke(request, body);
                return respond(request);
            }));
    }

    private static HttpResponseMessage JsonResponse(string body,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body)
        };
    }

    [Fact]
    public void DeleteRecipient_SendsDeleteRequestWithoutBody()
    {
        HttpRequestMessage? captured = null;
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent),
            (request, b) =>
            {
                captured = request;
                body = b;
            });

        client.DeleteRecipient(
            new DeleteRecipientRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/recipient/rcp_123", captured.RequestUri!.AbsolutePath);
        Assert.Null(body);
    }

    [Fact]
    public async Task DeleteRecipientAsync_SendsDeleteRequestWithoutBody()
    {
        HttpRequestMessage? captured = null;
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent),
            (request, b) =>
            {
                captured = request;
                body = b;
            });

        await client.DeleteRecipientAsync(
            new DeleteRecipientRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/recipient/rcp_123", captured.RequestUri!.AbsolutePath);
        Assert.Null(body);
    }

    [Fact]
    public void RequestRecipientSignature_PostsWithoutBodyAndParsesPendingSignature()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "name": "Anna Smith",
              "createdAt": "2026-02-13T15:56:12.607Z",
              "signature": {
                "status": "pending",
                "requestedAt": "2026-10-30T09:15:00.000Z",
                "useCount": 0
              }
            }
            """;

        HttpRequestMessage? captured = null;
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody, HttpStatusCode.Created),
            (request, b) =>
            {
                captured = request;
                body = b;
            });

        PdfGateRecipient recipient = client.RequestRecipientSignature(
            new RequestRecipientSignatureRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/recipient/rcp_123/signature/request",
            captured.RequestUri!.AbsolutePath);
        using JsonDocument requestJson = JsonDocument.Parse(body!);
        Assert.Equal(JsonValueKind.Object, requestJson.RootElement.ValueKind);
        Assert.Empty(requestJson.RootElement.EnumerateObject());

        Assert.Equal("rcp_123", recipient.Id);
        Assert.NotNull(recipient.Signature);
        Assert.Equal(StoredSignatureStatus.Pending,
            recipient.Signature.Status);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 30, 9, 15, 0, 0, TimeSpan.Zero),
            recipient.Signature.RequestedAt);
        Assert.Equal(0, recipient.Signature.UseCount);
        Assert.Null(recipient.Signature.AdoptionMethod);
        Assert.Null(recipient.Signature.EnrolledAt);
        Assert.Null(recipient.Signature.RevokedAt);
        Assert.Null(recipient.Signature.LastUsedAt);
        Assert.Null(recipient.Signature.Image);
    }

    [Fact]
    public async Task RequestRecipientSignatureAsync_PostsToSignatureRequestRoute()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "signature": { "status": "pending", "useCount": 0 }
            }
            """;

        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody, HttpStatusCode.Created),
            (request, _) => captured = request);

        PdfGateRecipient recipient = await client.RequestRecipientSignatureAsync(
            new RequestRecipientSignatureRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("/recipient/rcp_123/signature/request",
            captured.RequestUri!.AbsolutePath);
        Assert.Equal(StoredSignatureStatus.Pending,
            recipient.Signature!.Status);
    }

    [Fact]
    public void GetRecipient_ParsesActiveSignature()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "name": "Anna Smith",
              "signature": {
                "status": "active",
                "requestedAt": "2026-10-30T09:15:00.000Z",
                "adoptionMethod": "draw_signature",
                "enrolledAt": "2026-10-30T10:02:30.500Z",
                "lastUsedAt": "2026-10-31T08:00:00.000Z",
                "useCount": 3,
                "image": "data:image/png;base64,iVBORw0KGgo="
              }
            }
            """;

        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody));

        PdfGateRecipient recipient = client.GetRecipient(
            new GetRecipientRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        PdfGateRecipientStoredSignature signature = recipient.Signature!;
        Assert.Equal(StoredSignatureStatus.Active, signature.Status);
        Assert.Equal(SignatureAdoptionMethod.DrawSignature,
            signature.AdoptionMethod);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 30, 9, 15, 0, 0, TimeSpan.Zero),
            signature.RequestedAt);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 30, 10, 2, 30, 500, TimeSpan.Zero),
            signature.EnrolledAt);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 31, 8, 0, 0, 0, TimeSpan.Zero),
            signature.LastUsedAt);
        Assert.Equal(3, signature.UseCount);
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", signature.Image);
        Assert.Null(signature.RevokedAt);
    }

    [Fact]
    public void GetRecipient_NullsSignatureWhenAbsent()
    {
        const string responseBody = """
            { "id": "rcp_123", "email": "anna@example.com" }
            """;

        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody));

        PdfGateRecipient recipient = client.GetRecipient(
            new GetRecipientRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Null(recipient.Signature);
    }

    [Fact]
    public void GetRecipient_UnknownSignatureEnumValuesAreSurfacedAsNull()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "signature": {
                "status": "something_new",
                "adoptionMethod": "scan_signature",
                "useCount": 1
              }
            }
            """;

        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody));

        PdfGateRecipient recipient = client.GetRecipient(
            new GetRecipientRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.NotNull(recipient.Signature);
        Assert.Null(recipient.Signature.Status);
        Assert.Null(recipient.Signature.AdoptionMethod);
        Assert.Equal(1, recipient.Signature.UseCount);
    }

    [Fact]
    public void RemoveRecipientSignature_SendsDeleteAndParsesRevokedSignature()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "signature": {
                "status": "revoked",
                "requestedAt": "2026-10-30T09:15:00.000Z",
                "adoptionMethod": "type_signature",
                "enrolledAt": "2026-10-30T10:02:30.500Z",
                "revokedAt": "2026-11-01T12:00:00.000Z",
                "useCount": 2
              }
            }
            """;

        HttpRequestMessage? captured = null;
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody),
            (request, b) =>
            {
                captured = request;
                body = b;
            });

        PdfGateRecipient recipient = client.RemoveRecipientSignature(
            new RemoveRecipientSignatureRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/recipient/rcp_123/signature",
            captured.RequestUri!.AbsolutePath);
        Assert.Null(body);

        PdfGateRecipientStoredSignature signature = recipient.Signature!;
        Assert.Equal(StoredSignatureStatus.Revoked, signature.Status);
        Assert.Equal(SignatureAdoptionMethod.TypeSignature,
            signature.AdoptionMethod);
        Assert.Equal(
            new DateTimeOffset(2026, 11, 1, 12, 0, 0, 0, TimeSpan.Zero),
            signature.RevokedAt);
        Assert.Equal(2, signature.UseCount);
        Assert.Null(signature.Image);
    }

    [Fact]
    public async Task RemoveRecipientSignatureAsync_SendsDeleteToSignatureRoute()
    {
        const string responseBody = """
            {
              "id": "rcp_123",
              "email": "anna@example.com",
              "signature": { "status": "revoked", "useCount": 0 }
            }
            """;

        HttpRequestMessage? captured = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody),
            (request, _) => captured = request);

        PdfGateRecipient recipient = await client.RemoveRecipientSignatureAsync(
            new RemoveRecipientSignatureRequest { Id = "rcp_123" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Equal("/recipient/rcp_123/signature",
            captured.RequestUri!.AbsolutePath);
        Assert.Equal(StoredSignatureStatus.Revoked,
            recipient.Signature!.Status);
    }

    [Fact]
    public void CreateEnvelope_SerializesAutoSignAndSendSignedCopy()
    {
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(EmptyEnvelopeResponseBody),
            (_, b) => body = b);

        client.CreateEnvelope(
            new CreateEnvelopeRequest
            {
                RequesterName = "HR",
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
                                SigningOrder = 1,
                                SendSignedCopy = false
                            },
                            new EnvelopeRecipient
                            {
                                RecipientId = "rcp_123",
                                SigningOrder = 2,
                                AutoSign = true,
                                SendSignedCopy = true
                            }
                        ]
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        using JsonDocument json = JsonDocument.Parse(body!);
        JsonElement recipients = json.RootElement.GetProperty("documents")[0]
            .GetProperty("recipients");
        Assert.False(recipients[0].TryGetProperty("autoSign", out _));
        Assert.False(recipients[0].GetProperty("sendSignedCopy").GetBoolean());
        Assert.True(recipients[1].GetProperty("autoSign").GetBoolean());
        Assert.True(recipients[1].GetProperty("sendSignedCopy").GetBoolean());
        Assert.Equal("rcp_123",
            recipients[1].GetProperty("recipientId").GetString());
    }

    [Fact]
    public void CreateEnvelope_OmitsAutoSignAndSendSignedCopyWhenUnset()
    {
        string? body = null;
        using PdfGateClient client = CreateClient(
            _ => JsonResponse(EmptyEnvelopeResponseBody),
            (_, b) => body = b);

        client.CreateEnvelope(
            new CreateEnvelopeRequest
            {
                RequesterName = "HR",
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
        Assert.False(recipient.TryGetProperty("autoSign", out _));
        Assert.False(recipient.TryGetProperty("sendSignedCopy", out _));
    }

    [Fact]
    public void CreateEnvelope_ParsesAutoSignAndSendSignedCopyOnResponseRecipients()
    {
        const string responseBody = """
            {
              "id": "env_1",
              "status": "created",
              "documents": [
                {
                  "sourceDocumentId": "doc_1",
                  "status": "pending",
                  "recipients": [
                    {
                      "email": "a@example.com",
                      "status": "pending",
                      "signingOrder": 1,
                      "autoSign": false,
                      "sendSignedCopy": true,
                      "fields": []
                    },
                    {
                      "email": "ceo@example.com",
                      "recipientId": "rcp_123",
                      "status": "pending",
                      "signingOrder": 2,
                      "autoSign": true,
                      "sendSignedCopy": false,
                      "fields": []
                    }
                  ]
                }
              ],
              "createdAt": "2026-02-13T15:56:12.607Z"
            }
            """;

        using PdfGateClient client = CreateClient(
            _ => JsonResponse(responseBody, HttpStatusCode.Created));

        PdfGateEnvelope envelope = client.CreateEnvelope(
            new CreateEnvelopeRequest
            {
                RequesterName = "HR",
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
                                RecipientId = "rcp_123",
                                SigningOrder = 2,
                                AutoSign = true
                            }
                        ]
                    }
                ]
            },
            TestContext.Current.CancellationToken);

        IReadOnlyList<EnvelopeRecipientResponse> recipients =
            envelope.Documents[0].Recipients;
        Assert.False(recipients[0].AutoSign);
        Assert.True(recipients[0].SendSignedCopy);
        Assert.True(recipients[1].AutoSign);
        Assert.False(recipients[1].SendSignedCopy);
        Assert.Equal("rcp_123", recipients[1].RecipientId);
    }

    [Fact]
    public void WebhookEventType_RecipientSignatureEvents_RoundTrip()
    {
        Assert.Equal("\"recipient.signature.enrolled\"",
            JsonSerializer.Serialize(
                WebhookEventType.RecipientSignatureEnrolled));
        Assert.Equal("\"recipient.signature.revoked\"",
            JsonSerializer.Serialize(
                WebhookEventType.RecipientSignatureRevoked));

        Assert.Equal(WebhookEventType.RecipientSignatureEnrolled,
            JsonSerializer.Deserialize<WebhookEventType>(
                "\"recipient.signature.enrolled\""));
        Assert.Equal(WebhookEventType.RecipientSignatureRevoked,
            JsonSerializer.Deserialize<WebhookEventType>(
                "\"recipient.signature.revoked\""));
    }
}
