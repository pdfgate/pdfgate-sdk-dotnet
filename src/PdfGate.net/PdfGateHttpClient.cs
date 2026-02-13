using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

    public PdfGateHttpClient(string apiKey, Uri baseAddress,
        JsonSerializerOptions jsonOptions, int maxConcurrency) : this(apiKey,
        baseAddress,
        BuildHttpClientWithMaxConcurrency(maxConcurrency), jsonOptions)
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

    private static HttpClient BuildHttpClientWithMaxConcurrency(
        int maxConcurrency)
    {
        var handler = new SocketsHttpHandler
        {
            MaxConnectionsPerServer =
                maxConcurrency // max concurrent connections per host
        };

        var httpClient = new HttpClient(handler);

        return httpClient;
    }

    public async Task<string> PostAsJsonAsync(string url,
        string request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var content =
            new StringContent(request, Encoding.UTF8, "application/json");

        return await TrySendRequest(async () => await _httpClient
            .PostAsync(url, content, cancellationToken)
            .ConfigureAwait(false), url, cancellationToken);
    }

    public async Task<string> PostAsMultipartAsync(string url,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        return await TrySendRequest(
            async () => await _httpClient
                .PostAsync(url, content, cancellationToken)
                .ConfigureAwait(false), url, cancellationToken);
    }

    public async Task<Stream> GetStreamAsync(string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content
                    .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw PdfGateException.FromHttpError(response.StatusCode,
                    url, errorResponse);
            }

            var byteArrayContent = await response.Content
                .ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            Stream content = new MemoryStream(byteArrayContent, false);

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

    public async Task<string> GetAsync(string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        return await TrySendRequest(
            async () => await _httpClient
                .GetAsync(url, cancellationToken)
                .ConfigureAwait(false), url, cancellationToken);
    }

    private async Task<string> TrySendRequest(
        Func<Task<HttpResponseMessage>> sendRequest,
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await sendRequest();

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
