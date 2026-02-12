using System.Net;
using System.Text;
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

        var request = new GeneratePdfRequest { Html = "<p>hello</p>" };
        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.PostAsJsonAsync(endpoint,
                JsonSerializer.Serialize(request),
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

        var request = new GeneratePdfRequest { Html = "<p>hello</p>" };
        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.PostAsJsonAsync("v1/generate/pdf",
                JsonSerializer.Serialize(request),
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

        var request = new GeneratePdfRequest { Html = "<p>hello</p>" };
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            client.PostAsJsonAsync("v1/generate/pdf",
                JsonSerializer.Serialize(request),
                cts.Token));
    }

    [Fact]
    public async Task
        GetStreamAsync_WhenResponseIsNonSuccess_ThrowsPdfGateExceptionWithStatusBodyAndEndpoint()
    {
        const string endpoint = "file/somedocumentid";
        const string responseBody = "{\"error\":\"not found\"}";

        using PdfGateHttpClient client = CreateClient(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(responseBody)
            }));

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.GetStreamAsync(endpoint,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal(responseBody, exception.ResponseBody);
        Assert.Contains(endpoint, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task
        GetStreamAsync_WhenResponseIsSuccess_ReturnsStreamWithAllContent()
    {
        const string endpoint = "file/somedocumentid";
        var expectedBytes = Encoding.UTF8.GetBytes("pdf content");

        using PdfGateHttpClient client = CreateClient(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedBytes)
            }));

        await using Stream stream = await client.GetStreamAsync(endpoint,
            TestContext.Current.CancellationToken);
        using var actualContent = new MemoryStream();
        await stream.CopyToAsync(actualContent,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedBytes, actualContent.ToArray());
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
