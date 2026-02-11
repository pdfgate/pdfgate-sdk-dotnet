using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    private readonly HttpClient _httpClient;

    /// <summary>
    ///     Creates a new <see cref="PdfGate" /> instance.
    /// </summary>
    /// <param name="apiKey">The API key for the PDFGate HTTP API; either sandbox or production.</param>
    public PdfGate(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("An API key is required.");

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = GetBaseUriFromApiKey(apiKey);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
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

        try
        {
            using HttpResponseMessage response = await _httpClient
                .PostAsJsonAsync("v1/generate/pdf", request, JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw PdfGateException.FromHttpError(response.StatusCode,
                    "v1/generate/pdf", content);

            var document =
                JsonSerializer.Deserialize<PdfGateDocumentResponse>(content,
                    JsonOptions);
            if (document is null || document.Status is null)
                throw new PdfGateException(
                    "The API returned an invalid response for endpoint 'v1/generate/pdf'.");

            return document;
        }
        catch (PdfGateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfGateException(
                "Failed to call endpoint 'v1/generate/pdf'.", ex);
        }
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

        try
        {
            using MultipartFormDataContent form =
                PdfGateRequestBuilder.BuildFlattenPdfFormData(request,
                    JsonOptions);

            using HttpResponseMessage response = await _httpClient
                .PostAsync("forms/flatten", form, cancellationToken)
                .ConfigureAwait(false);

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw PdfGateException.FromHttpError(response.StatusCode,
                    "forms/flatten", content);

            var document =
                JsonSerializer.Deserialize<PdfGateDocumentResponse>(content,
                    JsonOptions);
            if (document is null || document.Status is null)
                throw new PdfGateException(
                    "The API returned an invalid response for endpoint 'forms/flatten'.");

            return document;
        }
        catch (PdfGateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfGateException(
                "Failed to call endpoint 'forms/flatten'.", ex);
        }
    }
}
