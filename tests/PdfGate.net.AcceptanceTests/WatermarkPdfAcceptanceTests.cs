using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Acceptance tests for watermark PDF operations against the live API.
/// </summary>
[Collection(AcceptanceTestCollection.Name)]
public sealed class
    WatermarkPdfAcceptanceTests : IClassFixture<PdfGateDocumentFixture>
{
    private static readonly byte[] WatermarkImage = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7Z1ioAAAAASUVORK5CYII=");

    private readonly PdfGateClientFixture _clientFixture;
    private readonly PdfGateDocumentFixture _documentFixture;

    /// <summary>
    ///     Initializes the test class with a shared acceptance fixture.
    /// </summary>
    public WatermarkPdfAcceptanceTests(PdfGateClientFixture clientFixture,
        PdfGateDocumentFixture documentFixture)
    {
        _clientFixture = clientFixture;
        _documentFixture = documentFixture;
    }

    /// <summary>
    ///     Applies a text watermark by existing document ID and verifies completion status.
    /// </summary>
    [Fact]
    public async Task
        WatermarkPdfAsync_ByDocumentIdWithText_ReturnsCompletedStatus()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);
        PdfGateFile fontFile = CreateFontFileOrSkip();

        var request = new WatermarkPdfRequest
        {
            DocumentId = source.Id,
            Type = WatermarkPdfType.Text,
            Text = "CONFIDENTIAL",
            FontFile = fontFile,
            Font = WatermarkPdfFont.HelveticaBold,
            FontSize = 30,
            FontColor = "rgb(156, 50, 168)",
            Opacity = 0.2,
            XPosition = 120,
            YPosition = 180,
            Rotate = 30
        };

        PdfGateDocumentResponse watermarked = await client.WatermarkPdfAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, watermarked.Status);
        Assert.Equal(DocumentType.Watermarked, watermarked.Type);
        Assert.Equal(source.Id, watermarked.DerivedFrom);
    }

    /// <summary>
    ///     Applies a text watermark by existing document ID and verifies completion status.
    /// </summary>
    [Fact]
    public void WatermarkPdf_ByDocumentIdWithText_ReturnsCompletedStatus()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            _documentFixture.GetDocumentOrSkip(client);
        PdfGateFile fontFile = CreateFontFileOrSkip();

        var request = new WatermarkPdfRequest
        {
            DocumentId = source.Id,
            Type = WatermarkPdfType.Text,
            Text = "CONFIDENTIAL",
            FontFile = fontFile,
            Font = WatermarkPdfFont.HelveticaBold,
            FontSize = 30,
            FontColor = "rgb(156, 50, 168)",
            Opacity = 0.2,
            XPosition = 120,
            YPosition = 180,
            Rotate = 30
        };

        PdfGateDocumentResponse watermarked = client.WatermarkPdf(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, watermarked.Status);
        Assert.Equal(DocumentType.Watermarked, watermarked.Type);
        Assert.Equal(source.Id, watermarked.DerivedFrom);
    }

    /// <summary>
    ///     Applies an image watermark by existing document ID and verifies completion status.
    /// </summary>
    [Fact]
    public async Task
        WatermarkPdfAsync_ByDocumentIdWithImage_ReturnsCompletedStatus()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            await _documentFixture.GetDocumentOrSkipAsync(client);

        var request = new WatermarkPdfRequest
        {
            DocumentId = source.Id,
            Type = WatermarkPdfType.Image,
            Watermark = new PdfGateFile
            {
                Name = "watermark.png",
                Type = "image/png",
                Content = new MemoryStream(WatermarkImage)
            },
            Opacity = 0.4,
            XPosition = 72,
            YPosition = 120,
            ImageWidth = 48,
            ImageHeight = 48,
            Rotate = 25
        };

        PdfGateDocumentResponse watermarked = await client.WatermarkPdfAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, watermarked.Status);
        Assert.Equal(DocumentType.Watermarked, watermarked.Type);
        Assert.Equal(source.Id, watermarked.DerivedFrom);
    }

    /// <summary>
    ///     Applies an image watermark by existing document ID and verifies completion status.
    /// </summary>
    [Fact]
    public void WatermarkPdf_ByDocumentIdWithImage_ReturnsCompletedStatus()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        PdfGateDocumentResponse source =
            _documentFixture.GetDocumentOrSkip(client);

        var request = new WatermarkPdfRequest
        {
            DocumentId = source.Id,
            Type = WatermarkPdfType.Image,
            Watermark = new PdfGateFile
            {
                Name = "watermark.png",
                Type = "image/png",
                Content = new MemoryStream(WatermarkImage)
            },
            Opacity = 0.4,
            XPosition = 72,
            YPosition = 120,
            ImageWidth = 48,
            ImageHeight = 48,
            Rotate = 25
        };

        PdfGateDocumentResponse watermarked = client.WatermarkPdf(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(DocumentStatus.Completed, watermarked.Status);
        Assert.Equal(DocumentType.Watermarked, watermarked.Type);
        Assert.Equal(source.Id, watermarked.DerivedFrom);
    }

    /// <summary>
    ///     Returns a PdfGateException with the HTTP API message when the API returns an error status.
    /// </summary>
    [Fact]
    public async Task
        WatermarkPdfAsync_WhenApiReturnsError_ThrowsPdfGateExceptionWithApiMessage()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        var request = new WatermarkPdfRequest
        {
            DocumentId = "invalid-id",
            Type = WatermarkPdfType.Text,
            Text = "CONFIDENTIAL"
        };

        var exception = await Assert.ThrowsAsync<PdfGateException>(() =>
            client.WatermarkPdfAsync(request,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.ResponseBody));
        Assert.Contains(exception.ResponseBody!, exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    ///     Returns a PdfGateException with the HTTP API message when the API returns an error status.
    /// </summary>
    [Fact]
    public void WatermarkPdf_WhenApiReturnsError_ThrowsPdfGateExceptionWithApiMessage()
    {
        PdfGateClient client = _clientFixture.GetClientOrSkip();
        var request = new WatermarkPdfRequest
        {
            DocumentId = "invalid-id",
            Type = WatermarkPdfType.Text,
            Text = "CONFIDENTIAL"
        };

        var exception = Assert.Throws<PdfGateException>(() =>
            client.WatermarkPdf(request,
                TestContext.Current.CancellationToken));

        Assert.NotNull(exception.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(exception.ResponseBody));
        Assert.Contains(exception.ResponseBody!, exception.Message,
            StringComparison.Ordinal);
    }

    private static PdfGateFile CreateFontFileOrSkip()
    {
        string[] candidatePaths = [];
        if (OperatingSystem.IsWindows())
            candidatePaths = ["C:\\Windows\\Fonts\\arial.ttf"];
        else if (OperatingSystem.IsMacOS())
            candidatePaths =
            [
                "/Library/Fonts/Arial.ttf",
                "/System/Library/Fonts/Supplemental/Arial.ttf"
            ];
        else
            Assert.Skip(
                "No local TTF font file found to run watermark fontFile acceptance test.");

        foreach (var path in candidatePaths)
        {
            if (!File.Exists(path))
                continue;

            return new PdfGateFile
            {
                Name = Path.GetFileName(path),
                Content = File.OpenRead(path)
            };
        }

        Assert.Skip(
            "No local TTF font file found to run watermark fontFile acceptance test.");

        return default!;
    }
}
