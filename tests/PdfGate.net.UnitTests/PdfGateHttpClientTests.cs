using System.Net;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class PdfGateHttpClientTests
{
    [Fact]
    public async Task
        PostAsJsonAsync_WhenResponseIsNonSuccess_ThrowsPdfGateExceptionWithStatusBodyAndEndpoint()
    {
        const string endpoint = "v1/generate/pdf";
        const string responseBody = "{\"error\":\"invalid input\"}";

        using PdfGateHttpClient client = CreateClient(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(responseBody)
            }));

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.PostAsJsonAsync(endpoint,
                new GeneratePdfRequest { Html = "<p>hello</p>" },
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Equal(responseBody, exception.ResponseBody);
        Assert.Contains(endpoint, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task
        PostAsJsonAsync_WhenUnexpectedExceptionOccurs_WrapsExceptionAndPreservesInnerException()
    {
        var innerException = new InvalidOperationException("send failed");

        using PdfGateHttpClient client = CreateClient(_ =>
            Task.FromException<HttpResponseMessage>(innerException));

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.PostAsJsonAsync("v1/generate/pdf",
                new GeneratePdfRequest { Html = "<p>hello</p>" },
                TestContext.Current.CancellationToken));

        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public async Task
        PostAsJsonAsync_WhenOperationIsCancelled_ThrowsOperationCancelledException()
    {
        using PdfGateHttpClient client = CreateClient(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent("")
            }));
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            client.PostAsJsonAsync("v1/generate/pdf",
                new GeneratePdfRequest { Html = "<p>hello</p>" },
                cts.Token));
    }

    private static PdfGateHttpClient CreateClient(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new DelegateHttpMessageHandler(sendAsync);
        return new PdfGateHttpClient(
            "live_test_key",
            new Uri("https://api.pdfgate.com/"),
            handler, new JsonSerializerOptions());
    }

    private sealed class DelegateHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return sendAsync(request);
        }
    }
}
