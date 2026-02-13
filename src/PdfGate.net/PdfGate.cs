using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Client used to interact with the PDFGate HTTP API.
/// </summary>
public sealed class PdfGate : IDisposable
{
    private static readonly Uri ProductionApiUri =
        new("https://api.pdfgate.com/");

    private static readonly Uri SandboxApiUri =
        new("https://api-sandbox.pdfgate.com/");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private readonly PdfGateHttpClient _httpClient;
    private readonly PdfGateResponseParser _responseParser;

    public PdfGate(string apiKey, int maxConcurrency)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        Uri baseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient = new PdfGateHttpClient(apiKey, baseAddress, JsonOptions,
            maxConcurrency);
        _responseParser = new PdfGateResponseParser(JsonOptions);
    }

    /// <summary>
    ///     Creates a new <see cref="PdfGate" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; either sandbox or production.</param>
    public PdfGate(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        Uri baseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient = new PdfGateHttpClient(apiKey, baseAddress, JsonOptions);
        _responseParser = new PdfGateResponseParser(JsonOptions);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private static Uri GetBaseUriFromApiKey(string apiKey)
    {
        if (apiKey.StartsWith("live_", StringComparison.Ordinal))
            return ProductionApiUri;

        if (apiKey.StartsWith("test_", StringComparison.Ordinal))
            return SandboxApiUri;

        throw new ArgumentException(
            "Invalid API key format. Expected to start with 'live_' or 'test_'.",
            nameof(apiKey));
    }

    /// <summary>
    ///     Generates a PDF document from HTML or URL input.
    /// </summary>
    /// <param name="request">Generate PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> GeneratePdfAsync(
        GeneratePdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "v1/generate/pdf";
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url,
            jsonRequest,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }


    /// <summary>
    ///     Flattens a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Flatten PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flattened document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> FlattenPdfAsync(
        FlattenPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "forms/flatten";
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Applies a watermark to a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Watermark PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> WatermarkPdfAsync(
        WatermarkPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "watermark/pdf";
        MultipartFormDataContent form =
            BuildWatermarkPdfMultipartRequest(request);
        var content = await _httpClient.PostAsMultipartAsync(url, form,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }

    private MultipartFormDataContent BuildWatermarkPdfMultipartRequest(
        WatermarkPdfRequest request)
    {
        var form = new MultipartFormDataContent
        {
            {
                new StringContent(request.DocumentId, Encoding.UTF8),
                "documentId"
            },
            {
                new StringContent(request.Type.ToString().ToLowerInvariant(),
                    Encoding.UTF8),
                "type"
            },
            { new StringContent("true", Encoding.UTF8), "jsonResponse" }
        };

        if (request.Watermark is not null)
        {
            var watermarkContent = new StreamContent(request.Watermark.Content);
            watermarkContent.Headers.ContentType =
                new MediaTypeHeaderValue(request.Watermark.Type);
            form.Add(watermarkContent, "watermark", request.Watermark.Name);
        }

        if (request.FontFile is not null)
        {
            var fontFileContent = new StreamContent(request.FontFile.Content);
            fontFileContent.Headers.ContentType =
                new MediaTypeHeaderValue(request.FontFile.Type);
            form.Add(fontFileContent, "fontFile", request.FontFile.Name);
        }

        if (request.Text is not null)
            form.Add(new StringContent(request.Text, Encoding.UTF8), "text");
        if (request.Font is not null)
            form.Add(
                new StringContent(request.Font.Value.ToApiValue(),
                    Encoding.UTF8), "font");
        if (request.FontSize.HasValue)
            form.Add(
                new StringContent(request.FontSize.Value.ToString(),
                    Encoding.UTF8), "fontSize");
        if (request.FontColor is not null)
            form.Add(
                new StringContent(request.FontColor, Encoding.UTF8),
                "fontColor");
        if (request.Opacity.HasValue)
            form.Add(
                new StringContent(request.Opacity.Value.ToString(
                    CultureInfo.InvariantCulture), Encoding.UTF8),
                "opacity");
        if (request.XPosition.HasValue)
            form.Add(
                new StringContent(request.XPosition.Value.ToString(),
                    Encoding.UTF8), "xPosition");
        if (request.YPosition.HasValue)
            form.Add(
                new StringContent(request.YPosition.Value.ToString(),
                    Encoding.UTF8), "yPosition");
        if (request.ImageWidth.HasValue)
            form.Add(
                new StringContent(request.ImageWidth.Value.ToString(),
                    Encoding.UTF8), "imageWidth");
        if (request.ImageHeight.HasValue)
            form.Add(
                new StringContent(request.ImageHeight.Value.ToString(),
                    Encoding.UTF8), "imageHeight");
        if (request.Rotate.HasValue)
            form.Add(
                new StringContent(request.Rotate.Value.ToString(
                    CultureInfo.InvariantCulture), Encoding.UTF8),
                "rotate");
        if (request.PreSignedUrlExpiresIn.HasValue)
            form.Add(
                new StringContent(
                    request.PreSignedUrlExpiresIn.Value.ToString(),
                    Encoding.UTF8), "preSignedUrlExpiresIn");
        if (request.Metadata is not null)
            form.Add(
                new StringContent(
                    JsonSerializer.Serialize(request.Metadata, JsonOptions),
                    Encoding.UTF8), "metadata");

        return form;
    }

    /// <summary>
    ///     Protects a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Protect PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Protected document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> ProtectPdfAsync(
        ProtectPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "protect/pdf";
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Extracts PDF form fields and their values.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public async Task<JsonElement> ExtractPdfFormDataAsync(
        ExtractPdfFormDataRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "forms/extract-data";
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cancellationToken);

        return _responseParser.ParseObject(content, url);
    }

    /// <summary>
    ///     Gets a file by its ID
    /// </summary>
    /// <param name="request">GetFile request payload with the ID of the document to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream to the file's content.</returns>
    public async Task<Stream> GetFileAsync(
        GetFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = $"file/{request.DocumentId}";
        Stream content =
            await _httpClient.GetStreamAsync(url, cancellationToken);

        return content;
    }

    /// <summary>
    ///     Upload a PDF file so you can apply any transformation to it.
    ///     The stream passed in the request is owned by the caller so it must
    ///     be disposed by the caller.
    /// </summary>
    /// <param name="request">UploadFile request payload holds the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document metadata to use to operate on it</returns>
    public async Task<PdfGateDocumentResponse> UploadFileAsync(
        UploadFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = "upload";
        var form = new MultipartFormDataContent();
        if (request.Url is not null)
        {
            form.Add(new StringContent(request.Url.AbsoluteUri, Encoding.UTF8),
                "url");
        }
        else
        {
            var fileContent = new StreamContent(request.Content);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");
            form.Add(fileContent, "file", "input.pdf");
        }

        if (request.Metadata is not null)
            form.Add(new StringContent(
                JsonSerializer.Serialize(request.Metadata, JsonOptions),
                Encoding.UTF8), "metadata");

        if (request.PreSignedUrlExpiresIn.HasValue)
            form.Add(
                new StringContent(
                    request.PreSignedUrlExpiresIn.Value.ToString(),
                    Encoding.UTF8),
                "preSignedUrlExpiresIn");


        var response =
            await _httpClient.PostAsMultipartAsync(url, form,
                cancellationToken);

        return _responseParser.Parse(response, url);
    }
}
