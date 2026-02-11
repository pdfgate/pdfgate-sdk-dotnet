using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Wrapper around the HttpClient that adds error handling
/// </summary>
internal sealed class PdfGateHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PdfGateHttpClient(string apiKey, Uri baseAddress,
        JsonSerializerOptions jsonOptions)
        : this(apiKey, baseAddress, new HttpClient(), jsonOptions)
    {
    }

    internal PdfGateHttpClient(string apiKey, Uri baseAddress,
        HttpMessageHandler httpMessageHandler,
        JsonSerializerOptions jsonOptions)
        : this(apiKey, baseAddress, new HttpClient(httpMessageHandler),
            jsonOptions)
    {
    }

    private PdfGateHttpClient(string apiKey, Uri baseAddress,
        HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClient = httpClient;
        _httpClient.BaseAddress = baseAddress;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _jsonOptions = jsonOptions;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<string> PostAsJsonAsync(string url,
        GeneratePdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await TryPost(async () => await _httpClient
            .PostAsJsonAsync(url, request, _jsonOptions,
                cancellationToken)
            .ConfigureAwait(false), url, cancellationToken);
    }

    public async Task<string> PostAsync(string url,
        FlattenPdfRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using MultipartFormDataContent form =
            PdfGateRequestBuilder.BuildFlattenPdfFormData(request,
                _jsonOptions);

        return await TryPost(async () => await _httpClient
            .PostAsync(url, form, cancellationToken)
            .ConfigureAwait(false), url, cancellationToken);
    }

    private async Task<string> TryPost(
        Func<Task<HttpResponseMessage>> sendPostRequest,
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await sendPostRequest();

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw PdfGateException.FromHttpError(response.StatusCode,
                    url, content);

            return content;
        }
        catch (PdfGateException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfGateException(
                $"Failed to call endpoint '{url}'.", ex);
        }
    }
}
