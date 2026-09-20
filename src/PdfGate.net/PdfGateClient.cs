using System.Text.Json;
using System.Text.Json.Serialization;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Client used to interact with the PDFGate HTTP API.
///     Full documentation of the API: https://pdfgate.com/documentation
///     There are 3 types of operations you can perform, and they usually follow
///     this order:
///     1. Create a PDF to operate on: GeneratePdf, and UploadFile will store a
///     new PDF that you can then operate on, and return its Document ID so you
///     can reference it on your transformation requests.
///     2. Transform a PDF: methods like FlattenPdf, ExtractPdfFormData,
///     WatermarkPdf, etc. will transform a PDF in a meaningful way and return
///     the Document ID of the result so you can download it.
///     3. Download a PDF: after you made the transformations you needed on a
///     PDF you can download it with GetFile.
/// </summary>
public sealed class PdfGateClient : IDisposable
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

    internal PdfGateClient(string apiKey, HttpMessageHandler httpMessageHandler)
        : this(apiKey, httpMessageHandler, new PdfGateRequestTimeouts())
    {
    }

    internal PdfGateClient(string apiKey, HttpMessageHandler httpMessageHandler,
        PdfGateRequestTimeouts requestTimeouts)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        Guard.ThrowIfNull(httpMessageHandler);
        Guard.ThrowIfNull(requestTimeouts);
        requestTimeouts.Validate();

        Uri baseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient = new PdfGateHttpClient(apiKey, baseAddress,
            httpMessageHandler, JsonOptions);
        _responseParser = new PdfGateResponseParser(JsonOptions);
        _requestTimeouts = requestTimeouts;
    }

    /// <summary>
    ///     Creates a new <see cref="PdfGateClient" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; "test_" for sandbox or "live_" for production.</param>
    public PdfGateClient(string apiKey)
        : this(apiKey, new PdfGateRequestTimeouts())
    {
    }

    /// <summary>
    ///     Creates a new <see cref="PdfGateClient" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; either sandbox or production.</param>
    /// <param name="requestTimeouts">Per-endpoint request timeout configuration.</param>
    internal PdfGateClient(string apiKey, PdfGateRequestTimeouts requestTimeouts)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        Guard.ThrowIfNull(requestTimeouts);
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
    ///     Generates a PDF document from an inline HTML or a URL.
    /// </summary>
    /// <param name="request">Generate PDF request payload. See <see cref="GeneratePdfRequest" />.</param>
    /// <returns>Generated document metadata response.</returns>
    public PdfGateDocumentResponse GeneratePdf(
        GeneratePdfRequest request)
    {
        return GeneratePdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Generates a PDF document from an inline HTML or a URL.
    /// </summary>
    /// <param name="request">Generate PDF request payload. See <see cref="GeneratePdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated document metadata response.</returns>
    public PdfGateDocumentResponse GeneratePdf(
        GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Generates a PDF document from an inline HTML or a URL.
    /// </summary>
    /// <param name="request">Generate PDF request payload. See <see cref="GeneratePdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> GeneratePdfAsync(
        GeneratePdfRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    ///     Creates a signing envelope from previously created source documents.
    ///     Each recipient is given either as an email and name, or as the
    ///     recipientId of a stored recipient. Embedded recipients receive no
    ///     email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending.
    /// </summary>
    /// <param name="request">Create envelope request payload. See <see cref="CreateEnvelopeRequest" />.</param>
    /// <returns>Created envelope metadata response.</returns>
    public PdfGateEnvelope CreateEnvelope(
        CreateEnvelopeRequest request)
    {
        return CreateEnvelope(request, CancellationToken.None);
    }

    /// <summary>
    ///     Creates a signing envelope from previously created source documents.
    ///     Each recipient is given either as an email and name, or as the
    ///     recipientId of a stored recipient. Embedded recipients receive no
    ///     email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending.
    /// </summary>
    /// <param name="request">Create envelope request payload. See <see cref="CreateEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created envelope metadata response.</returns>
    public PdfGateEnvelope CreateEnvelope(
        CreateEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateEnvelope;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Creates a signing envelope from previously created source documents.
    ///     Each recipient is given either as an email and name, or as the
    ///     recipientId of a stored recipient. Embedded recipients receive no
    ///     email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending.
    /// </summary>
    /// <param name="request">Create envelope request payload. See <see cref="CreateEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created envelope metadata response.</returns>
    public async Task<PdfGateEnvelope> CreateEnvelopeAsync(
        CreateEnvelopeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateEnvelope;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Sends a previously created envelope to all recipients. Embedded
    ///     recipients receive no email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending. On documents with a signingOrder only the first
    ///     recipients are emailed; later recipients are activated as earlier
    ///     ones sign.
    /// </summary>
    /// <param name="request">Send envelope request payload. See <see cref="SendEnvelopeRequest" />.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public PdfGateEnvelope SendEnvelope(
        SendEnvelopeRequest request)
    {
        return SendEnvelope(request, CancellationToken.None);
    }

    /// <summary>
    ///     Sends a previously created envelope to all recipients. Embedded
    ///     recipients receive no email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending. On documents with a signingOrder only the first
    ///     recipients are emailed; later recipients are activated as earlier
    ///     ones sign.
    /// </summary>
    /// <param name="request">Send envelope request payload. See <see cref="SendEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public PdfGateEnvelope SendEnvelope(
        SendEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.SendEnvelope(request.Id);
        var content = _httpClient.PostAsJson(url, "{}",
            cts.Token);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Sends a previously created envelope to all recipients. Embedded
    ///     recipients receive no email and get their signing links via
    ///     <see cref="CreateEmbedLink(CreateEmbedLinkRequest)" /> after
    ///     sending. On documents with a signingOrder only the first
    ///     recipients are emailed; later recipients are activated as earlier
    ///     ones sign.
    /// </summary>
    /// <param name="request">Send envelope request payload. See <see cref="SendEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public async Task<PdfGateEnvelope> SendEnvelopeAsync(
        SendEnvelopeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.SendEnvelope(request.Id);
        var content = await _httpClient.PostAsJsonAsync(url, "{}",
            cts.Token).ConfigureAwait(false);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Gets the current state of an envelope by its identifier.
    /// </summary>
    /// <param name="request">Get envelope request payload. See <see cref="GetEnvelopeRequest" />.</param>
    /// <returns>Current envelope metadata response.</returns>
    public PdfGateEnvelope GetEnvelope(
        GetEnvelopeRequest request)
    {
        return GetEnvelope(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets the current state of an envelope by its identifier.
    /// </summary>
    /// <param name="request">Get envelope request payload. See <see cref="GetEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current envelope metadata response.</returns>
    public PdfGateEnvelope GetEnvelope(
        GetEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetEnvelope(request.Id);
        var content = _httpClient.Get(url, cts.Token);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Gets the current state of an envelope by its identifier.
    /// </summary>
    /// <param name="request">Get envelope request payload. See <see cref="GetEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current envelope metadata response.</returns>
    public async Task<PdfGateEnvelope> GetEnvelopeAsync(
        GetEnvelopeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetEnvelope(request.Id);
        var content = await _httpClient.GetAsync(url, cts.Token)
            .ConfigureAwait(false);

        return _responseParser.ParseEnvelope(content, url);
    }


    /// <summary>
    ///     Flatten an interactive PDF into a static, non-editable PDF.
    /// </summary>
    /// <param name="request">Flatten PDF request payload. See <see cref="FlattenPdfRequest" />.</param>
    /// <returns>Flattened document metadata response.</returns>
    public PdfGateDocumentResponse FlattenPdf(
        FlattenPdfRequest request)
    {
        return FlattenPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Flatten an interactive PDF into a static, non-editable PDF.
    /// </summary>
    /// <param name="request">Flatten PDF request payload. See <see cref="FlattenPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flattened document metadata response.</returns>
    public PdfGateDocumentResponse FlattenPdf(
        FlattenPdfRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Flatten an interactive PDF into a static, non-editable PDF.
    /// </summary>
    /// <param name="request">Flatten PDF request payload. See <see cref="FlattenPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Flattened document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> FlattenPdfAsync(
        FlattenPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    ///     Apply a text or image watermark to a PDF.
    /// </summary>
    /// <param name="request">Watermark PDF request payload. See <see cref="WatermarkPdfRequest" />.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public PdfGateDocumentResponse WatermarkPdf(
        WatermarkPdfRequest request)
    {
        return WatermarkPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Apply a text or image watermark to a PDF.
    /// </summary>
    /// <param name="request">Watermark PDF request payload. See <see cref="WatermarkPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public PdfGateDocumentResponse WatermarkPdf(
        WatermarkPdfRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Apply a text or image watermark to a PDF.
    /// </summary>
    /// <param name="request">Watermark PDF request payload. See <see cref="WatermarkPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Watermarked document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> WatermarkPdfAsync(
        WatermarkPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    ///     Protect a PDF by requiring a password to view it.
    ///     It can further restrict usage of the PDF by disabling:
    ///     - printing
    ///     - copying
    ///     - editing
    ///     The metadata of the document (title, author, etc.) can be kept
    ///     visible, even to users without the password, by disabling the param
    ///     `encryptMetadata` in the request.
    /// </summary>
    /// <param name="request">Protect PDF request payload. See <see cref="ProtectPdfRequest" />.</param>
    /// <returns>Protected document metadata response.</returns>
    public PdfGateDocumentResponse ProtectPdf(
        ProtectPdfRequest request)
    {
        return ProtectPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Protect a PDF by requiring a password to view it.
    ///     It can further restrict usage of the PDF by disabling:
    ///     - printing
    ///     - copying
    ///     - editing
    ///     The metadata of the document (title, author, etc.) can be kept
    ///     visible, even to users without the password, by disabling the param
    ///     `encryptMetadata` in the request.
    /// </summary>
    /// <param name="request">Protect PDF request payload. See <see cref="ProtectPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Protected document metadata response.</returns>
    public PdfGateDocumentResponse ProtectPdf(
        ProtectPdfRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Protect a PDF by requiring a password to view it.
    ///     It can further restrict usage of the PDF by disabling:
    ///     - printing
    ///     - copying
    ///     - editing
    ///     The metadata of the document (title, author, etc.) can be kept
    ///     visible, even to users without the password, by disabling the param
    ///     `encryptMetadata` in the request.
    /// </summary>
    /// <param name="request">Protect PDF request payload. See <see cref="ProtectPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Protected document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> ProtectPdfAsync(
        ProtectPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    ///     Reduce a PDF's file size without changing its visual appearence.
    ///     Enable linearization to allow faster streaming when serving the file
    ///     over the network.
    /// </summary>
    /// <param name="request">Compress PDF request payload. See <see cref="CompressPdfRequest" />.</param>
    /// <returns>Compressed document metadata response.</returns>
    public PdfGateDocumentResponse CompressPdf(
        CompressPdfRequest request)
    {
        return CompressPdf(request, CancellationToken.None);
    }

    /// <summary>
    ///     Reduce a PDF's file size without changing its visual appearence.
    ///     Enable linearization to allow faster streaming when serving the file
    ///     over the network.
    /// </summary>
    /// <param name="request">Compress PDF request payload. See <see cref="CompressPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed document metadata response.</returns>
    public PdfGateDocumentResponse CompressPdf(
        CompressPdfRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Reduce a PDF's file size without changing its visual appearence.
    ///     Enable linearization to allow faster streaming when serving the file
    ///     over the network.
    /// </summary>
    /// <param name="request">Compress PDF request payload. See <see cref="CompressPdfRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Compressed document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> CompressPdfAsync(
        CompressPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    ///     Extract form field data from a fillable PDF and return it as JSON.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload. See <see cref="ExtractPdfFormDataRequest" />.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public JsonElement ExtractPdfFormData(
        ExtractPdfFormDataRequest request)
    {
        return ExtractPdfFormData(request, CancellationToken.None);
    }

    /// <summary>
    ///     Extract form field data from a fillable PDF and return it as JSON.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload. See <see cref="ExtractPdfFormDataRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public JsonElement ExtractPdfFormData(
        ExtractPdfFormDataRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    ///     Extract form field data from a fillable PDF and return it as JSON.
    /// </summary>
    /// <param name="request">Extract PDF form data request payload. See <see cref="ExtractPdfFormDataRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON object containing extracted form field values.</returns>
    public async Task<JsonElement> ExtractPdfFormDataAsync(
        ExtractPdfFormDataRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">
    ///     GetDocument request payload with the ID of the document to retrieve. See
    ///     <see cref="GetDocumentRequest" />.
    /// </param>
    /// <returns>Document metadata.</returns>
    public PdfGateDocumentResponse GetDocument(
        GetDocumentRequest request)
    {
        return GetDocument(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets metadata for a document by ID.
    /// </summary>
    /// <param name="request">
    ///     GetDocument request payload with the ID of the document to retrieve. See
    ///     <see cref="GetDocumentRequest" />.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Document metadata.</returns>
    public PdfGateDocumentResponse GetDocument(
        GetDocumentRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">
    ///     GetDocument request payload with the ID of the document to retrieve. See
    ///     <see cref="GetDocumentRequest" />.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Document metadata.</returns>
    public async Task<PdfGateDocumentResponse> GetDocumentAsync(
        GetDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">
    ///     GetFile request payload with the ID of the document to download. See
    ///     <see cref="GetFileRequest" />.
    /// </param>
    /// <returns>A stream to the file's content.</returns>
    public Stream GetFile(
        GetFileRequest request)
    {
        return GetFile(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets a file by its ID
    /// </summary>
    /// <param name="request">
    ///     GetFile request payload with the ID of the document to download. See
    ///     <see cref="GetFileRequest" />.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream to the file's content.</returns>
    public Stream GetFile(
        GetFileRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">
    ///     GetFile request payload with the ID of the document to download. See
    ///     <see cref="GetFileRequest" />.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A stream to the file's content.</returns>
    public async Task<Stream> GetFileAsync(
        GetFileRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">UploadFile request payload holds the file. See <see cref="UploadFileRequest" />.</param>
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
    /// <param name="request">UploadFile request payload holds the file. See <see cref="UploadFileRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document metadata to use to operate on it</returns>
    public PdfGateDocumentResponse UploadFile(
        UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

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
    /// <param name="request">UploadFile request payload holds the file. See <see cref="UploadFileRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document metadata to use to operate on it</returns>
    public async Task<PdfGateDocumentResponse> UploadFileAsync(
        UploadFileRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

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

    /// <summary>
    ///     Add interactive form fields to a PDF.
    /// </summary>
    /// <param name="request">Add form fields request payload. See <see cref="AddFormFieldsRequest" />.</param>
    /// <returns>Resulting document metadata response.</returns>
    public PdfGateDocumentResponse AddFormFields(
        AddFormFieldsRequest request)
    {
        return AddFormFields(request, CancellationToken.None);
    }

    /// <summary>
    ///     Add interactive form fields to a PDF.
    /// </summary>
    /// <param name="request">Add form fields request payload. See <see cref="AddFormFieldsRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resulting document metadata response.</returns>
    public PdfGateDocumentResponse AddFormFields(
        AddFormFieldsRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.FlattenPdf,
            cancellationToken);
        var url = ApiRoutes.AddFormFields;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Add interactive form fields to a PDF.
    /// </summary>
    /// <param name="request">Add form fields request payload. See <see cref="AddFormFieldsRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resulting document metadata response.</returns>
    public async Task<PdfGateDocumentResponse> AddFormFieldsAsync(
        AddFormFieldsRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts = CreateTimeoutTokenSource(
            _requestTimeouts.FlattenPdf,
            cancellationToken);
        var url = ApiRoutes.AddFormFields;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse(content, url);
    }

    /// <summary>
    ///     Voids (cancels) an envelope in created or in_progress status.
    ///     Recipients who have not signed yet are notified by email and their
    ///     signing links stop working; documents already signed by all
    ///     recipients are not affected. The optional reason is visible to
    ///     recipients. This action cannot be undone.
    /// </summary>
    /// <param name="request">Void envelope request payload. See <see cref="VoidEnvelopeRequest" />.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public PdfGateEnvelope VoidEnvelope(
        VoidEnvelopeRequest request)
    {
        return VoidEnvelope(request, CancellationToken.None);
    }

    /// <summary>
    ///     Voids (cancels) an envelope in created or in_progress status.
    ///     Recipients who have not signed yet are notified by email and their
    ///     signing links stop working; documents already signed by all
    ///     recipients are not affected. The optional reason is visible to
    ///     recipients. This action cannot be undone.
    /// </summary>
    /// <param name="request">Void envelope request payload. See <see cref="VoidEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public PdfGateEnvelope VoidEnvelope(
        VoidEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.VoidEnvelope(request.Id);
        var content = _httpClient.PostAsJson(url, BuildVoidEnvelopeBody(request),
            cts.Token);

        return _responseParser.ParseEnvelope(content, url);
    }

    /// <summary>
    ///     Voids (cancels) an envelope in created or in_progress status.
    ///     Recipients who have not signed yet are notified by email and their
    ///     signing links stop working; documents already signed by all
    ///     recipients are not affected. The optional reason is visible to
    ///     recipients. This action cannot be undone.
    /// </summary>
    /// <param name="request">Void envelope request payload. See <see cref="VoidEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated envelope metadata response.</returns>
    public async Task<PdfGateEnvelope> VoidEnvelopeAsync(
        VoidEnvelopeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.VoidEnvelope(request.Id);
        var content = await _httpClient.PostAsJsonAsync(url,
            BuildVoidEnvelopeBody(request), cts.Token).ConfigureAwait(false);

        return _responseParser.ParseEnvelope(content, url);
    }

    private static string BuildVoidEnvelopeBody(VoidEnvelopeRequest request)
    {
        return request.Reason is null
            ? "{}"
            : JsonSerializer.Serialize(new { reason = request.Reason },
                JsonOptions);
    }

    /// <summary>
    ///     Permanently deletes an envelope and the files it produced.
    ///     The signed documents and audit logs are removed from storage,
    ///     recipient data is anonymized, and recipients lose access. Source
    ///     documents are not deleted. Only envelopes in draft, completed,
    ///     expired, or voided status can be deleted — void an active envelope
    ///     first. This action cannot be undone.
    /// </summary>
    /// <param name="request">Delete envelope request payload. See <see cref="DeleteEnvelopeRequest" />.</param>
    public void DeleteEnvelope(
        DeleteEnvelopeRequest request)
    {
        DeleteEnvelope(request, CancellationToken.None);
    }

    /// <summary>
    ///     Permanently deletes an envelope and the files it produced.
    ///     The signed documents and audit logs are removed from storage,
    ///     recipient data is anonymized, and recipients lose access. Source
    ///     documents are not deleted. Only envelopes in draft, completed,
    ///     expired, or voided status can be deleted — void an active envelope
    ///     first. This action cannot be undone.
    /// </summary>
    /// <param name="request">Delete envelope request payload. See <see cref="DeleteEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void DeleteEnvelope(
        DeleteEnvelopeRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.DeleteEnvelope(request.Id);
        _httpClient.Delete(url, cts.Token);
    }

    /// <summary>
    ///     Permanently deletes an envelope and the files it produced.
    ///     The signed documents and audit logs are removed from storage,
    ///     recipient data is anonymized, and recipients lose access. Source
    ///     documents are not deleted. Only envelopes in draft, completed,
    ///     expired, or voided status can be deleted — void an active envelope
    ///     first. This action cannot be undone.
    /// </summary>
    /// <param name="request">Delete envelope request payload. See <see cref="DeleteEnvelopeRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteEnvelopeAsync(
        DeleteEnvelopeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.DeleteEnvelope(request.Id);
        await _httpClient.DeleteAsync(url, cts.Token).ConfigureAwait(false);
    }

    /// <summary>
    ///     Creates an embed link for an embedded recipient to sign inside
    ///     your own application. The envelope must be in in_progress status.
    ///     The link expires after 10 minutes, so create it when the signer is
    ///     ready — one link per signing session. When the session ends the
    ///     iframe redirects to the returnUrl with event (signing_complete,
    ///     voided, expired or not_found), envelopeId, documentId and
    ///     recipientId appended as query parameters, preserving the
    ///     returnUrl's existing query parameters. On documents with a
    ///     signingOrder the link can only be created once it is the
    ///     recipient's turn (the API returns an error before that); the
    ///     envelope.recipient.activated webhook signals that moment.
    /// </summary>
    /// <param name="request">Create embed link request payload. See <see cref="CreateEmbedLinkRequest" />.</param>
    /// <returns>Embed link metadata response.</returns>
    public PdfGateEmbedLink CreateEmbedLink(
        CreateEmbedLinkRequest request)
    {
        return CreateEmbedLink(request, CancellationToken.None);
    }

    /// <summary>
    ///     Creates an embed link for an embedded recipient to sign inside
    ///     your own application. The envelope must be in in_progress status.
    ///     The link expires after 10 minutes, so create it when the signer is
    ///     ready — one link per signing session. When the session ends the
    ///     iframe redirects to the returnUrl with event (signing_complete,
    ///     voided, expired or not_found), envelopeId, documentId and
    ///     recipientId appended as query parameters, preserving the
    ///     returnUrl's existing query parameters. On documents with a
    ///     signingOrder the link can only be created once it is the
    ///     recipient's turn (the API returns an error before that); the
    ///     envelope.recipient.activated webhook signals that moment.
    /// </summary>
    /// <param name="request">Create embed link request payload. See <see cref="CreateEmbedLinkRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embed link metadata response.</returns>
    public PdfGateEmbedLink CreateEmbedLink(
        CreateEmbedLinkRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateEmbedLink(request.Id);
        var content = _httpClient.PostAsJson(url,
            BuildCreateEmbedLinkBody(request), cts.Token);

        return _responseParser.Parse<PdfGateEmbedLink>(content, url,
            embedLink => !string.IsNullOrEmpty(embedLink.Url));
    }

    /// <summary>
    ///     Creates an embed link for an embedded recipient to sign inside
    ///     your own application. The envelope must be in in_progress status.
    ///     The link expires after 10 minutes, so create it when the signer is
    ///     ready — one link per signing session. When the session ends the
    ///     iframe redirects to the returnUrl with event (signing_complete,
    ///     voided, expired or not_found), envelopeId, documentId and
    ///     recipientId appended as query parameters, preserving the
    ///     returnUrl's existing query parameters. On documents with a
    ///     signingOrder the link can only be created once it is the
    ///     recipient's turn (the API returns an error before that); the
    ///     envelope.recipient.activated webhook signals that moment.
    /// </summary>
    /// <param name="request">Create embed link request payload. See <see cref="CreateEmbedLinkRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embed link metadata response.</returns>
    public async Task<PdfGateEmbedLink> CreateEmbedLinkAsync(
        CreateEmbedLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateEmbedLink(request.Id);
        var content = await _httpClient.PostAsJsonAsync(url,
            BuildCreateEmbedLinkBody(request), cts.Token).ConfigureAwait(false);

        return _responseParser.Parse<PdfGateEmbedLink>(content, url,
            embedLink => !string.IsNullOrEmpty(embedLink.Url));
    }

    private static string BuildCreateEmbedLinkBody(
        CreateEmbedLinkRequest request)
    {
        return JsonSerializer.Serialize(new
        {
            documentId = request.DocumentId,
            recipientId = request.RecipientId,
            returnUrl = request.ReturnUrl
        }, JsonOptions);
    }

    /// <summary>
    ///     Permanently delete a stored document.
    ///     A document referenced by a draft or in-progress envelope cannot be
    ///     deleted until those envelopes are completed or expired.
    /// </summary>
    /// <param name="request">Delete document request payload. See <see cref="DeleteDocumentRequest" />.</param>
    public void DeleteDocument(
        DeleteDocumentRequest request)
    {
        DeleteDocument(request, CancellationToken.None);
    }

    /// <summary>
    ///     Permanently delete a stored document.
    ///     A document referenced by a draft or in-progress envelope cannot be
    ///     deleted until those envelopes are completed or expired.
    /// </summary>
    /// <param name="request">Delete document request payload. See <see cref="DeleteDocumentRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void DeleteDocument(
        DeleteDocumentRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.DeleteDocument(request.DocumentId);
        _httpClient.Delete(url, cts.Token);
    }

    /// <summary>
    ///     Permanently delete a stored document.
    ///     A document referenced by a draft or in-progress envelope cannot be
    ///     deleted until those envelopes are completed or expired.
    /// </summary>
    /// <param name="request">Delete document request payload. See <see cref="DeleteDocumentRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteDocumentAsync(
        DeleteDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.DeleteDocument(request.DocumentId);
        await _httpClient.DeleteAsync(url, cts.Token).ConfigureAwait(false);
    }

    /// <summary>
    ///     Creates a stored recipient that can be reused across envelopes via
    ///     its recipientId. The email is stored lowercased and cannot be
    ///     changed after creation. Emails are not unique: every call creates
    ///     a new recipient.
    /// </summary>
    /// <param name="request">Create recipient request payload. See <see cref="CreateRecipientRequest" />.</param>
    /// <returns>Created recipient metadata response.</returns>
    public PdfGateRecipient CreateRecipient(
        CreateRecipientRequest request)
    {
        return CreateRecipient(request, CancellationToken.None);
    }

    /// <summary>
    ///     Creates a stored recipient that can be reused across envelopes via
    ///     its recipientId. The email is stored lowercased and cannot be
    ///     changed after creation. Emails are not unique: every call creates
    ///     a new recipient.
    /// </summary>
    /// <param name="request">Create recipient request payload. See <see cref="CreateRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created recipient metadata response.</returns>
    public PdfGateRecipient CreateRecipient(
        CreateRecipientRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateRecipient;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest,
            cts.Token);

        return ParseRecipient(content, url);
    }

    /// <summary>
    ///     Creates a stored recipient that can be reused across envelopes via
    ///     its recipientId. The email is stored lowercased and cannot be
    ///     changed after creation. Emails are not unique: every call creates
    ///     a new recipient.
    /// </summary>
    /// <param name="request">Create recipient request payload. See <see cref="CreateRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created recipient metadata response.</returns>
    public async Task<PdfGateRecipient> CreateRecipientAsync(
        CreateRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.CreateRecipient;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return ParseRecipient(content, url);
    }

    /// <summary>
    ///     Lists stored recipients with the given email address, oldest
    ///     first. The lookup is case-insensitive.
    /// </summary>
    /// <param name="request">List recipients request payload. See <see cref="ListRecipientsRequest" />.</param>
    /// <returns>List of stored recipients matching the email.</returns>
    public PdfGateRecipientList ListRecipients(
        ListRecipientsRequest request)
    {
        return ListRecipients(request, CancellationToken.None);
    }

    /// <summary>
    ///     Lists stored recipients with the given email address, oldest
    ///     first. The lookup is case-insensitive.
    /// </summary>
    /// <param name="request">List recipients request payload. See <see cref="ListRecipientsRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stored recipients matching the email.</returns>
    public PdfGateRecipientList ListRecipients(
        ListRecipientsRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.ListRecipients(request.Email);
        var content = _httpClient.Get(url, cts.Token);

        return _responseParser.Parse<PdfGateRecipientList>(content, url);
    }

    /// <summary>
    ///     Lists stored recipients with the given email address, oldest
    ///     first. The lookup is case-insensitive.
    /// </summary>
    /// <param name="request">List recipients request payload. See <see cref="ListRecipientsRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stored recipients matching the email.</returns>
    public async Task<PdfGateRecipientList> ListRecipientsAsync(
        ListRecipientsRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.ListRecipients(request.Email);
        var content = await _httpClient.GetAsync(url, cts.Token)
            .ConfigureAwait(false);

        return _responseParser.Parse<PdfGateRecipientList>(content, url);
    }

    /// <summary>
    ///     Gets a stored recipient by its identifier.
    /// </summary>
    /// <param name="request">Get recipient request payload. See <see cref="GetRecipientRequest" />.</param>
    /// <returns>Stored recipient metadata response.</returns>
    public PdfGateRecipient GetRecipient(
        GetRecipientRequest request)
    {
        return GetRecipient(request, CancellationToken.None);
    }

    /// <summary>
    ///     Gets a stored recipient by its identifier.
    /// </summary>
    /// <param name="request">Get recipient request payload. See <see cref="GetRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stored recipient metadata response.</returns>
    public PdfGateRecipient GetRecipient(
        GetRecipientRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetRecipient(request.Id);
        var content = _httpClient.Get(url, cts.Token);

        return ParseRecipient(content, url);
    }

    /// <summary>
    ///     Gets a stored recipient by its identifier.
    /// </summary>
    /// <param name="request">Get recipient request payload. See <see cref="GetRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stored recipient metadata response.</returns>
    public async Task<PdfGateRecipient> GetRecipientAsync(
        GetRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetRecipient(request.Id);
        var content = await _httpClient.GetAsync(url, cts.Token)
            .ConfigureAwait(false);

        return ParseRecipient(content, url);
    }

    /// <summary>
    ///     Updates a stored recipient's name and/or metadata. The email
    ///     address cannot be changed. Existing envelopes are not affected:
    ///     they keep the recipient name they were created with.
    /// </summary>
    /// <param name="request">Update recipient request payload. See <see cref="UpdateRecipientRequest" />.</param>
    /// <returns>Updated recipient metadata response.</returns>
    public PdfGateRecipient UpdateRecipient(
        UpdateRecipientRequest request)
    {
        return UpdateRecipient(request, CancellationToken.None);
    }

    /// <summary>
    ///     Updates a stored recipient's name and/or metadata. The email
    ///     address cannot be changed. Existing envelopes are not affected:
    ///     they keep the recipient name they were created with.
    /// </summary>
    /// <param name="request">Update recipient request payload. See <see cref="UpdateRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated recipient metadata response.</returns>
    public PdfGateRecipient UpdateRecipient(
        UpdateRecipientRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetRecipient(request.Id);
        var content = _httpClient.PatchAsJson(url,
            BuildUpdateRecipientBody(request), cts.Token);

        return ParseRecipient(content, url);
    }

    /// <summary>
    ///     Updates a stored recipient's name and/or metadata. The email
    ///     address cannot be changed. Existing envelopes are not affected:
    ///     they keep the recipient name they were created with.
    /// </summary>
    /// <param name="request">Update recipient request payload. See <see cref="UpdateRecipientRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated recipient metadata response.</returns>
    public async Task<PdfGateRecipient> UpdateRecipientAsync(
        UpdateRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetRecipient(request.Id);
        var content = await _httpClient.PatchAsJsonAsync(url,
            BuildUpdateRecipientBody(request), cts.Token).ConfigureAwait(false);

        return ParseRecipient(content, url);
    }

    private static string BuildUpdateRecipientBody(
        UpdateRecipientRequest request)
    {
        return JsonSerializer.Serialize(new
        {
            name = request.Name,
            metadata = request.Metadata
        }, JsonOptions);
    }

    private PdfGateRecipient ParseRecipient(string content, string url)
    {
        return _responseParser.Parse<PdfGateRecipient>(content, url,
            recipient => !string.IsNullOrEmpty(recipient.Id));
    }

    /// <summary>
    ///     Register a webhook endpoint to receive PDFGate event notifications.
    ///     The response includes a secret (returned only once, at creation time)
    ///     used to verify webhook payloads via <see cref="PdfGateWebhook.VerifySignature(string, string, string)" />.
    /// </summary>
    /// <param name="request">Create webhook request payload. See <see cref="CreateWebhookRequest" />.</param>
    /// <returns>Created webhook metadata, including the signing secret.</returns>
    public PdfGateWebhookResponse CreateWebhook(
        CreateWebhookRequest request)
    {
        return CreateWebhook(request, CancellationToken.None);
    }

    /// <summary>
    ///     Register a webhook endpoint to receive PDFGate event notifications.
    /// </summary>
    /// <param name="request">Create webhook request payload. See <see cref="CreateWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created webhook metadata, including the signing secret.</returns>
    public PdfGateWebhookResponse CreateWebhook(
        CreateWebhookRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.Webhook;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = _httpClient.PostAsJson(url, jsonRequest, cts.Token);

        return _responseParser.Parse<PdfGateWebhookResponse>(content, url);
    }

    /// <summary>
    ///     Register a webhook endpoint to receive PDFGate event notifications.
    /// </summary>
    /// <param name="request">Create webhook request payload. See <see cref="CreateWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created webhook metadata, including the signing secret.</returns>
    public async Task<PdfGateWebhookResponse> CreateWebhookAsync(
        CreateWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.Webhook;
        var jsonRequest = JsonSerializer.Serialize(request, JsonOptions);
        var content = await _httpClient.PostAsJsonAsync(url, jsonRequest,
            cts.Token).ConfigureAwait(false);

        return _responseParser.Parse<PdfGateWebhookResponse>(content, url);
    }

    /// <summary>
    ///     Retrieve a registered webhook by ID. The secret is not returned by
    ///     this endpoint (only at creation time).
    /// </summary>
    /// <param name="request">Get webhook request payload. See <see cref="GetWebhookRequest" />.</param>
    /// <returns>Webhook metadata.</returns>
    public PdfGateWebhookResponse GetWebhook(
        GetWebhookRequest request)
    {
        return GetWebhook(request, CancellationToken.None);
    }

    /// <summary>
    ///     Retrieve a registered webhook by ID.
    /// </summary>
    /// <param name="request">Get webhook request payload. See <see cref="GetWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Webhook metadata.</returns>
    public PdfGateWebhookResponse GetWebhook(
        GetWebhookRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetWebhook(request.Id);
        var content = _httpClient.Get(url, cts.Token);

        return _responseParser.Parse<PdfGateWebhookResponse>(content, url);
    }

    /// <summary>
    ///     Retrieve a registered webhook by ID.
    /// </summary>
    /// <param name="request">Get webhook request payload. See <see cref="GetWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Webhook metadata.</returns>
    public async Task<PdfGateWebhookResponse> GetWebhookAsync(
        GetWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetWebhook(request.Id);
        var content = await _httpClient.GetAsync(url, cts.Token)
            .ConfigureAwait(false);

        return _responseParser.Parse<PdfGateWebhookResponse>(content, url);
    }

    /// <summary>
    ///     Delete a registered webhook.
    /// </summary>
    /// <param name="request">Delete webhook request payload. See <see cref="DeleteWebhookRequest" />.</param>
    public void DeleteWebhook(
        DeleteWebhookRequest request)
    {
        DeleteWebhook(request, CancellationToken.None);
    }

    /// <summary>
    ///     Delete a registered webhook.
    /// </summary>
    /// <param name="request">Delete webhook request payload. See <see cref="DeleteWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void DeleteWebhook(
        DeleteWebhookRequest request,
        CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetWebhook(request.Id);
        _httpClient.Delete(url, cts.Token);
    }

    /// <summary>
    ///     Delete a registered webhook.
    /// </summary>
    /// <param name="request">Delete webhook request payload. See <see cref="DeleteWebhookRequest" />.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteWebhookAsync(
        DeleteWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using CancellationTokenSource cts =
            CreateTimeoutTokenSource(_requestTimeouts.DefaultEndpoint,
                cancellationToken);
        var url = ApiRoutes.GetWebhook(request.Id);
        await _httpClient.DeleteAsync(url, cts.Token).ConfigureAwait(false);
    }
}
