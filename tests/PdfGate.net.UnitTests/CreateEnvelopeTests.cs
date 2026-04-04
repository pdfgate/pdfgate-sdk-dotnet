using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class CreateEnvelopeTests
{
    [Fact]
    public void CreateEnvelope_SerializesNestedPayloadAndParsesTypedResponse()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "created",
                                      "documents": [
                                        {
                                          "sourceDocumentId": "doc_123",
                                          "recipients": [
                                            {
                                              "email": "anna@example.com",
                                              "status": "pending",
                                              "fields": [
                                                {
                                                  "name": "signature",
                                                  "type": "signature"
                                                }
                                              ]
                                            }
                                          ],
                                          "status": "pending"
                                        }
                                      ],
                                      "createdAt": "2024-02-13T15:56:12.607Z",
                                      "metadata": {
                                        "customerId": "cus_123",
                                        "department": "sales"
                                      }
                                    }
                                    """;

        string? requestBody = null;
        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                };
            }));

        var request = new CreateEnvelopeRequest
        {
            RequesterName = "John Doe",
            Documents =
            [
                new EnvelopeDocument
                {
                    SourceDocumentId = "doc_123",
                    Name = "Employment Agreement",
                    Recipients =
                    [
                        new EnvelopeRecipient
                        {
                            Email = "anna@example.com",
                            Name = "Anna Smith",
                            Role = "signer"
                        }
                    ]
                }
            ],
            Metadata = new { CustomerId = "cus_123", Department = "sales" }
        };

        PdfGateEnvelope response = client.CreateEnvelope(request,
            TestContext.Current.CancellationToken);

        Assert.NotNull(requestBody);
        using JsonDocument json = JsonDocument.Parse(requestBody);
        JsonElement root = json.RootElement;
        Assert.Equal("John Doe", root.GetProperty("requesterName").GetString());

        JsonElement document = root.GetProperty("documents")[0];
        Assert.Equal("doc_123",
            document.GetProperty("sourceDocumentId").GetString());
        Assert.Equal("Employment Agreement",
            document.GetProperty("name").GetString());

        JsonElement recipient = document.GetProperty("recipients")[0];
        Assert.Equal("anna@example.com",
            recipient.GetProperty("email").GetString());
        Assert.Equal("Anna Smith", recipient.GetProperty("name").GetString());
        Assert.Equal("signer", recipient.GetProperty("role").GetString());

        Assert.Equal(EnvelopeStatus.Created, response.Status);
        Assert.Equal(EnvelopeDocumentStatus.Pending,
            response.Documents[0].Status);
        Assert.Equal(DocumentRecipientStatus.Pending,
            response.Documents[0].Recipients[0].Status);
        Assert.Equal(DocumentFieldType.Signature,
            response.Documents[0].Recipients[0].Fields[0].Type);
        Assert.True(response.Metadata.HasValue);
        JsonElement metadata = response.Metadata.GetValueOrDefault();
        Assert.Equal("cus_123",
            metadata.GetProperty("customerId").GetString());
    }

    [Fact]
    public void CreateEnvelope_OmitsOptionalFieldsWhenUnset()
    {
        string? requestBody = null;
        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\":\"env_123\",\"status\":\"created\",\"documents\":[],\"createdAt\":\"2024-02-13T15:56:12.607Z\"}")
                };
            }));

        var request = new CreateEnvelopeRequest
        {
            RequesterName = "John Doe",
            Documents =
            [
                new EnvelopeDocument
                {
                    SourceDocumentId = "doc_123",
                    Name = "Employment Agreement",
                    Recipients =
                    [
                        new EnvelopeRecipient
                        {
                            Email = "anna@example.com",
                            Name = "Anna Smith"
                        }
                    ]
                }
            ]
        };

        _ = client.CreateEnvelope(request,
            TestContext.Current.CancellationToken);

        Assert.NotNull(requestBody);
        using JsonDocument json = JsonDocument.Parse(requestBody);
        JsonElement root = json.RootElement;
        Assert.False(root.TryGetProperty("metadata", out _));

        JsonElement recipient =
            root.GetProperty("documents")[0].GetProperty("recipients")[0];
        Assert.False(recipient.TryGetProperty("role", out _));
    }

    [Fact]
    public async Task CreateEnvelopeAsync_ReturnsEnvelopeResponse()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "created",
                                      "documents": [
                                        {
                                          "sourceDocumentId": "doc_123",
                                          "recipients": [
                                            {
                                              "email": "anna@example.com",
                                              "status": "pending",
                                              "fields": []
                                            }
                                          ],
                                          "status": "pending"
                                        }
                                      ],
                                      "createdAt": "2024-02-13T15:56:12.607Z"
                                    }
                                    """;

        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                })));

        var request = new CreateEnvelopeRequest
        {
            RequesterName = "John Doe",
            Documents =
            [
                new EnvelopeDocument
                {
                    SourceDocumentId = "doc_123",
                    Name = "Employment Agreement",
                    Recipients =
                    [
                        new EnvelopeRecipient
                        {
                            Email = "anna@example.com",
                            Name = "Anna Smith"
                        }
                    ]
                }
            ]
        };

        PdfGateEnvelope response = await client.CreateEnvelopeAsync(request,
            TestContext.Current.CancellationToken);

        Assert.Equal("env_123", response.Id);
        Assert.Equal(EnvelopeStatus.Created, response.Status);
        Assert.Equal("doc_123", response.Documents[0].SourceDocumentId);
    }
}
