using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class SendEnvelopeTests
{
    [Fact]
    public void SendEnvelope_UsesEnvelopePathAndReturnsEnvelopeResponse()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "in_progress",
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

        string? requestPath = null;
        string? requestBody = null;
        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                requestPath = request.RequestUri?.PathAndQuery;
                requestBody = request.Content!.ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                };
            }));

        PdfGateEnvelope response = client.SendEnvelope(new SendEnvelopeRequest
        {
            Id = "env_123"
        }, TestContext.Current.CancellationToken);

        Assert.Equal("/envelope/env_123/send", requestPath);
        Assert.Equal("{}", requestBody);
        Assert.Equal(EnvelopeStatus.InProgress, response.Status);
        Assert.Equal("env_123", response.Id);
    }

    [Fact]
    public async Task SendEnvelopeAsync_ReturnsEnvelopeResponse()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "in_progress",
                                      "documents": [],
                                      "createdAt": "2024-02-13T15:56:12.607Z"
                                    }
                                    """;

        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                })));

        PdfGateEnvelope response = await client.SendEnvelopeAsync(
            new SendEnvelopeRequest
            {
                Id = "env_123"
            }, TestContext.Current.CancellationToken);

        Assert.Equal("env_123", response.Id);
        Assert.Equal(EnvelopeStatus.InProgress, response.Status);
    }
}
