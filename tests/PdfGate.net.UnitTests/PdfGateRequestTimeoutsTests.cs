using System.Net;
using System.Text;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class PdfGateRequestTimeoutsTests
{
    [Fact]
    public void Defaults_MatchEndpointTimeoutRequirements()
    {
        var timeouts = new PdfGateRequestTimeouts();

        Assert.Equal(TimeSpan.FromMinutes(15), timeouts.GeneratePdf);
        Assert.Equal(TimeSpan.FromMinutes(3), timeouts.FlattenPdf);
        Assert.Equal(TimeSpan.FromMinutes(3), timeouts.WatermarkPdf);
        Assert.Equal(TimeSpan.FromMinutes(3), timeouts.ProtectPdf);
        Assert.Equal(TimeSpan.FromMinutes(3), timeouts.CompressPdf);
        Assert.Equal(TimeSpan.FromSeconds(60), timeouts.DefaultEndpoint);
    }

    [Fact]
    public void Constructor_WhenTimeoutIsNotPositive_Throws()
    {
        var timeouts = new PdfGateRequestTimeouts
        {
            GeneratePdf = TimeSpan.Zero
        };

        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PdfGateClient("live_test_key", timeouts));

        Assert.Equal("GeneratePdf", exception.ParamName);
    }

    /*
     * Verify that the token that reaches the client is a wrapped token and not
     * the one passed by the user to the public API.
     *
     * It distinguishes them by passing CancellationToken.None as input, because
     * CancellationToken.None.CanBeCanceled is false. A linked token created via
     * CancellationTokenSource.CreateLinkedTokenSource(...) is cancelable,
     * so CanBeCanceled is true. In the test, each call uses
     * CancellationToken.None, and the fake handler records the actual token
     * that reaches HTTP. Therefore, if used token is cancelable it means it was
     * wrapped.
     */
    [Fact]
    public async Task AllPublicCancellationTokenOverloads_UseWrappedToken()
    {
        var handler = new RecordingHttpMessageHandler();
        using var client = new PdfGateClient("live_test_key", handler,
            new PdfGateRequestTimeouts());

        _ = client.GeneratePdf(new GeneratePdfRequest { Html = "<p>x</p>" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.GeneratePdfAsync(
            new GeneratePdfRequest { Html = "<p>x</p>" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.FlattenPdf(new FlattenPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.FlattenPdfAsync(
            new FlattenPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.WatermarkPdf(CreateWatermarkRequest(),
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.WatermarkPdfAsync(CreateWatermarkRequest(),
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.ProtectPdf(new ProtectPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.ProtectPdfAsync(
            new ProtectPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.CompressPdf(new CompressPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.CompressPdfAsync(
            new CompressPdfRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.ExtractPdfFormData(
            new ExtractPdfFormDataRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.ExtractPdfFormDataAsync(
            new ExtractPdfFormDataRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = client.GetDocument(new GetDocumentRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.GetDocumentAsync(
            new GetDocumentRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        using Stream getFileStream = client.GetFile(
            new GetFileRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);
        Assert.NotNull(getFileStream);

        await using Stream getFileAsyncStream = await client.GetFileAsync(
            new GetFileRequest { DocumentId = "doc-id" },
            CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);
        Assert.NotNull(getFileAsyncStream);

        _ = client.UploadFile(
            new UploadFileRequest
            {
                Url = new Uri("https://example.com/input.pdf")
            }, CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);

        _ = await client.UploadFileAsync(
            new UploadFileRequest
            {
                Url = new Uri("https://example.com/input.pdf")
            }, CancellationToken.None);
        Assert.True(handler.LastCancellationToken.CanBeCanceled);
    }

    private static WatermarkPdfRequest CreateWatermarkRequest()
    {
        return new WatermarkPdfRequest
        {
            DocumentId = "doc-id",
            Type = WatermarkPdfType.Text,
            Text = "watermark"
        };
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private const string DocumentResponseBody =
            "{\"id\":\"doc-id\",\"status\":\"completed\"}";

        private const string ExtractResponseBody =
            "{\"field\":\"value\"}";

        private static readonly byte[] FileResponseBody =
            Encoding.UTF8.GetBytes("file-bytes");

        public CancellationToken LastCancellationToken
        {
            get;
            private set;
        }

        protected override HttpResponseMessage Send(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastCancellationToken = cancellationToken;
            return CreateResponse(request);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(CreateResponse(request));
        }

        private static HttpResponseMessage CreateResponse(
            HttpRequestMessage request)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (path.EndsWith("/forms/extract-data", StringComparison.Ordinal))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ExtractResponseBody)
                };

            if (path.Contains("/file/", StringComparison.Ordinal))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(FileResponseBody)
                };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(DocumentResponseBody)
            };
        }
    }
}
