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
    private readonly PdfGateRequestTimeouts _requestTimeouts;
    private readonly PdfGateResponseParser _responseParser;

    internal PdfGate(string apiKey, HttpMessageHandler httpMessageHandler)
        : this(apiKey, httpMessageHandler, new PdfGateRequestTimeouts())
    {
    }

    internal PdfGate(string apiKey, HttpMessageHandler httpMessageHandler,
        PdfGateRequestTimeouts requestTimeouts)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        ArgumentNullException.ThrowIfNull(httpMessageHandler);
        ArgumentNullException.ThrowIfNull(requestTimeouts);
        requestTimeouts.Validate();

        Uri baseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient = new PdfGateHttpClient(apiKey, baseAddress,
            httpMessageHandler, JsonOptions);
        _responseParser = new PdfGateResponseParser(JsonOptions);
        _requestTimeouts = requestTimeouts;
    }

    /// <summary>
    ///     Creates a new <see cref="PdfGate" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; either sandbox or production.</param>
    public PdfGate(string apiKey)
        : this(apiKey, new PdfGateRequestTimeouts())
    {
    }

    /// <summary>
    ///     Creates a new <see cref="PdfGate" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; either sandbox or production.</param>
    /// <param name="requestTimeouts">Per-endpoint request timeout configuration.</param>
    internal PdfGate(string apiKey, PdfGateRequestTimeouts requestTimeouts)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        ArgumentNullException.ThrowIfNull(requestTimeouts);
        requestTimeouts.Validate();

        Uri baseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient = new PdfGateHttpClient(apiKey, baseAddress, JsonOptions);
        _responseParser = new PdfGateResponseParser(JsonOptions);
        _requestTimeouts = requestTimeouts;
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

    private static CancellationTokenSource CreateTimeoutTokenSource(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var cts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        return cts;
    }

    /// <summary>
    ///     Generates a PDF document from HTML or URL input.
    /// </summary>
    /// <param name="request">Generate PDF request payload.</param>
    /// <returns>Generated document metadata response.</returns>
    public PdfGateDocumentResponse GeneratePdf(
        GeneratePdfRequest request)
    {
        return GeneratePdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Generates a PDF document from HTML or URL input.
    /// </summary>
    /// <param name="request">Generate PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated document metadata response.</returns>
    public PdfGateDocumentResponse GeneratePdf(
        GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.GeneratePdf,
            cancellationToken);
        var url = ApiRoutes.GeneratePdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url,
            jsonRequest,
            cts.Token);

        return _responseParser.Parse(content, url);
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

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.GeneratePdf,
            cancellationToken);
        var url = ApiRoutes.GeneratePdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url,
            jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }


    /// <summary>
    ///     Flattens a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Flatten PDF request payload.</param>
    /// <returns>Flattened document metadata response.</returns>
    public PdfGateDocumentResponse FlattenPdf(
        FlattenPdfRequest request)
    {
        return FlattenPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Flattens a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Flatten PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flattened document metadata response.</returns>
    public PdfGateDocumentResponse FlattenPdf(
        FlattenPdfRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.FlattenPdf,
            cancellationToken);
        var url = ApiRoutes.FlattenPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

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

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.FlattenPdf,
            cancellationToken);
        var url = ApiRoutes.FlattenPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Applies a watermark to a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Watermark PDF request payload.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public PdfGateDocumentResponse WatermarkPdf(
        WatermarkPdfRequest request)
    {
        return WatermarkPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Applies a watermark to a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Watermark PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public PdfGateDocumentResponse WatermarkPdf(
        WatermarkPdfRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.WatermarkPdf,
            cancellationToken);
        var url = ApiRoutes.WatermarkPdf;
        var builder = new WatermarkPdfMultipartRequestBuilder(request,
            JsonOptions);
        MultipartFormDataContent form =
            builder.Build();
        var content = _httpClient.PostAsMultipart(url, form,
            cts.Token);

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

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.WatermarkPdf,
            cancellationToken);
        var url = ApiRoutes.WatermarkPdf;
        var builder = new WatermarkPdfMultipartRequestBuilder(request,
            JsonOptions);
        MultipartFormDataContent form =
            builder.Build();
        var content = await _httpClient.PostAsMultipartAsync(url, form,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Protects a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Protect PDF request payload.</param>
    /// <returns>Protected document metadata response.</returns>
    public PdfGateDocumentResponse ProtectPdf(
        ProtectPdfRequest request)
    {
        return ProtectPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Protects a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Protect PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Protected document metadata response.</returns>
    public PdfGateDocumentResponse ProtectPdf(
        ProtectPdfRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.ProtectPdf,
            cancellationToken);
        var url = ApiRoutes.ProtectPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

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

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.ProtectPdf,
            cancellationToken);
        var url = ApiRoutes.ProtectPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Compresses a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Compress PDF request payload.</param>
    /// <returns>Compressed document metadata response.</returns>
    public PdfGateDocumentResponse CompressPdf(
        CompressPdfRequest request)
    {
        return CompressPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Compresses a PDF and returns document metadata.
    /// </summary>
    /// <param name="request">Compress PDF request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed document metadata response.</returns>
    public PdfGateDocumentResponse CompressPdf(
        CompressPdfRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.CompressPdf,
            cancellationToken);
        var url = ApiRoutes.CompressPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

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

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.CompressPdf,
            cancellationToken);
        var url = ApiRoutes.CompressPdf;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Extracts PDF form fields and their values.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public JsonElement ExtractPdfFormData(
        ExtractPdfFormDataRequest request)
    {
        return ExtractPdfFormData(request, CancellationToken.None);
    }

    /// <summary>
    ///     Extracts PDF form fields and their values.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public JsonElement ExtractPdfFormData(
        ExtractPdfFormDataRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.ExtractPdfFormData;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

        return _responseParser.ParseObject(content, url);
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

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.ExtractPdfFormData;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.ParseObject(content, url);
    }

    /// <summary>
    ///     Gets metadata for a document by ID.
    /// </summary>
    /// <param name="request">GetDocument request payload with the ID of the document to retrieve.</param>
    /// <returns>Document metadata.</returns>
    public PdfGateDocumentResponse GetDocument(
        GetDocumentRequest request)
    {
        return GetDocument(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets metadata for a document by ID.
    /// </summary>
    /// <param name="request">GetDocument request payload with the ID of the document to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Document metadata.</returns>
    public PdfGateDocumentResponse GetDocument(
        GetDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetDocument(request.DocumentId,
            request.PreSignedUrlExpiresIn);

        var content = _httpClient.Get(url, cts.Token);

        return _responseParser.Parse(content, url);
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

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetDocument(request.DocumentId,
            request.PreSignedUrlExpiresIn);

        var content = await _httpClient.GetAsync(url, cts.Token)
            .ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Gets a file by its ID
    /// </summary>
    /// <param name="request">GetFile request payload with the ID of the document to download.</param>
    /// <returns>A stream to the file's content.</returns>
    public Stream GetFile(
        GetFileRequest request)
    {
        return GetFile(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets a file by its ID
    /// </summary>
    /// <param name="request">GetFile request payload with the ID of the document to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream to the file's content.</returns>
    public Stream GetFile(
        GetFileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetFile(request.DocumentId);
        Stream content =
            _httpClient.GetStream(url, cts.Token);

        return content;
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

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetFile(request.DocumentId);
        Stream content =
            await _httpClient.GetStreamAsync(url, cts.Token)
                .ConfigureAwait(false);

        return content;
    }

    /// <summary>
    ///     Upload a PDF file so you can apply any transformation to it.
    ///     The stream passed in the request is owned by the caller so it must
    ///     be disposed by the caller.
    /// </summary>
    /// <param name="request">UploadFile request payload holds the file.</param>
    /// <returns>The uploaded document metadata to use to operate on it</returns>
    public PdfGateDocumentResponse UploadFile(
        UploadFileRequest request)
    {
        return UploadFile(request, CancellationToken.None);
    }

    /// <summary>
    ///     Upload a PDF file so you can apply any transformation to it.
    ///     The stream passed in the request is owned by the caller so it must
    ///     be disposed by the caller.
    /// </summary>
    /// <param name="request">UploadFile request payload holds the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document metadata to use to operate on it</returns>
    public PdfGateDocumentResponse UploadFile(
        UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.UploadFile;
        var builder = new UploadFileMultipartRequestBuilder(request,
            JsonOptions);
        MultipartFormDataContent form = builder.Build();

        var response =
            _httpClient.PostAsMultipart(url, form,
                cts.Token);

        return _responseParser.Parse(response, url);
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

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.UploadFile;
        var builder = new UploadFileMultipartRequestBuilder(request,
            JsonOptions);
        MultipartFormDataContent form = builder.Build();

        var response =
            await _httpClient.PostAsMultipartAsync(url, form,
                cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(response, url);
    }
}
