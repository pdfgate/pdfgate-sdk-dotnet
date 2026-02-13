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

        var url = ApiRoutes.GeneratePdf;
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

        var url = ApiRoutes.FlattenPdf;
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

        var url = ApiRoutes.WatermarkPdf;
        var builder = new WatermarkPdfMultipartRequestBuilder(request,
            JsonOptions);
        MultipartFormDataContent form =
            builder.Build();
        var content = await _httpClient.PostAsMultipartAsync(url, form,
            cancellationToken);

        return _responseParser.Parse(content, url);
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

        var url = ApiRoutes.ProtectPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Compresses a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Compress PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> CompressPdfAsync(
        CompressPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = ApiRoutes.CompressPdf;
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

        var url = ApiRoutes.ExtractPdfFormData;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cancellationToken);

        return _responseParser.ParseObject(content, url);
    }

    /// <summary>
    ///     Gets metadata for a document by ID.
    /// </summary>
    /// <param name="request">GetDocument request payload with the ID of the document to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Document metadata.</returns>
    public async Task<PdfGateDocumentResponse> GetDocumentAsync(
        GetDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = ApiRoutes.GetDocument(request.DocumentId,
            request.PreSignedUrlExpiresIn);

        var content = await _httpClient.GetAsync(url, cancellationToken);

        return _responseParser.Parse(content, url);
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

        var url = ApiRoutes.GetFile(request.DocumentId);
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

        var url = ApiRoutes.UploadFile;
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
