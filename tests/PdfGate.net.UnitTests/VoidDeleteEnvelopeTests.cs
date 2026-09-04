using System.Net;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class VoidDeleteEnvelopeTests
{
    [Fact]
    public void VoidEnvelope_SendsReasonAndReturnsVoidedEnvelope()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "voided",
                                      "documents": [
                                        {
                                          "sourceDocumentId": "doc_123",
                                          "recipients": [
                                            {
                                              "email": "anna@example.com",
                                              "status": "voided",
                                              "fields": []
                                            }
                                          ],
                                          "status": "voided"
                                        }
                                      ],
                                      "createdAt": "2024-02-13T15:56:12.607Z",
                                      "voidedAt": "2024-02-20T09:12:45.101Z",
                                      "voidReason": "Contract terms changed"
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

        PdfGateEnvelope response = client.VoidEnvelope(new VoidEnvelopeRequest
        {
            Id = "env_123",
            Reason = "Contract terms changed"
        }, TestContext.Current.CancellationToken);

        Assert.Equal("/envelope/env_123/void", requestPath);
        Assert.Equal("""{"reason":"Contract terms changed"}""", requestBody);
        Assert.Equal(EnvelopeStatus.Voided, response.Status);
        Assert.Equal("Contract terms changed", response.VoidReason);
        Assert.NotNull(response.VoidedAt);
        Assert.Equal(EnvelopeDocumentStatus.Voided,
            response.Documents[0].Status);
        Assert.Equal(DocumentRecipientStatus.Voided,
            response.Documents[0].Recipients[0].Status);
    }

    [Fact]
    public async Task VoidEnvelopeAsync_SendsEmptyBodyWithoutReason()
    {
        const string responseBody = """
                                    {
                                      "id": "env_123",
                                      "status": "voided",
                                      "documents": [],
                                      "createdAt": "2024-02-13T15:56:12.607Z"
                                    }
                                    """;

        string? requestBody = null;
        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                requestBody = request.Content!.ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseBody)
                });
            }));

        PdfGateEnvelope response = await client.VoidEnvelopeAsync(
            new VoidEnvelopeRequest
            {
                Id = "env_123"
            }, TestContext.Current.CancellationToken);

        Assert.Equal("{}", requestBody);
        Assert.Equal(EnvelopeStatus.Voided, response.Status);
    }

    [Fact]
    public void DeleteEnvelope_SendsDeleteToEnvelopePath()
    {
        string? requestPath = null;
        HttpMethod? requestMethod = null;
        using var client = new PdfGateClient("live_test_key",
            new TestHttpMessageHandler((request, _) =>
            {
                requestPath = request.RequestUri?.PathAndQuery;
                requestMethod = request.Method;

                return new HttpResponseMessage(HttpStatusCode.OK);
            }));

        client.DeleteEnvelope(new DeleteEnvelopeRequest
        {
            Id = "env_123"
        }, TestContext.Current.CancellationToken);

        Assert.Equal("/envelope/env_123", requestPath);
        Assert.Equal(HttpMethod.Delete, requestMethod);
    }
}
