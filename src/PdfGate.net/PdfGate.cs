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
        var content = await _httpClient.PostAsJsonAsync(url,
            request,
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
        var content = await _httpClient.PostAsync(url, request,
            cancellationToken);

        return _responseParser.Parse(content, url);
    }
}
