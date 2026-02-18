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
        Guard.ThrowIfNull(httpClient);

        _httpClient = httpClient;
        _httpClient.BaseAddress = baseAddress;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        /*
         * HttpClient's default timeout is 100 seconds. Since we need longer
         * timeouts for some requests, e.g. 15 minutes for GeneratePdf, the
         * global timeout of the client needs to be longer than the longest
         * per-request timeout because the client uses the shortest of both.
         */
        _httpClient.Timeout = new TimeSpan(0, 30, 0);
        _jsonOptions = jsonOptions;
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
    }


    public async Task<string> PostAsJsonAsync(string url,
        string request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        return await TrySendRequest(async () =>
            {
                using var content =
                    new StringContent(request, Encoding.UTF8,
                        "application/json");
                return await _httpClient
                    .PostAsync(url, content, cancellationToken)
                    .ConfigureAwait(false);
            }, url, cancellationToken)
            .ConfigureAwait(false);
    }

    public string PostAsJson(string url,
        string request,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        return TrySendRequest(() =>
        {
            using var content =
                new StringContent(request, Encoding.UTF8,
                    "application/json");
            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = content;
            return _httpClient.SendAsync(requestMessage, cancellationToken)
                .GetAwaiter()
                .GetResult();
        }, url, cancellationToken);
    }

    public async Task<string> PostAsMultipartAsync(string url,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(content);

        return await TrySendRequest(
                async () => await _httpClient
                    .PostAsync(url, content, cancellationToken)
                    .ConfigureAwait(false), url, cancellationToken)
            .ConfigureAwait(false);
    }

    public string PostAsMultipart(string url,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(content);

        return TrySendRequest(() =>
        {
            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = content;
            return _httpClient.SendAsync(requestMessage, cancellationToken)
                .GetAwaiter()
                .GetResult();
        }, url, cancellationToken);
    }

    public async Task<Stream> GetStreamAsync(string url,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(url);

        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(url, cancellationToken)
                    .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content
                    .ReadAsStringAsync().ConfigureAwait(false);
                throw PdfGateException.FromHttpError(response.StatusCode,
                    url, errorResponse);
            }

            var byteArrayContent = await response.Content
                .ReadAsByteArrayAsync().ConfigureAwait(false);
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
        Guard.ThrowIfNull(url);

        return await TrySendRequest(
                async () => await _httpClient
                    .GetAsync(url, cancellationToken)
                    .ConfigureAwait(false), url, cancellationToken)
            .ConfigureAwait(false);
    }

    public string Get(string url,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(url);

        return TrySendRequest(
            () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    url);
                return _httpClient.SendAsync(request, cancellationToken)
                    .GetAwaiter()
                    .GetResult();
            },
            url, cancellationToken);
    }

    public Stream GetStream(string url,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(url);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using HttpResponseMessage response =
                _httpClient.SendAsync(request, cancellationToken)
                    .GetAwaiter()
                    .GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = ReadContentAsString(response.Content,
                    cancellationToken);
                throw PdfGateException.FromHttpError(response.StatusCode,
                    url, errorResponse);
            }

            var byteArrayContent = ReadContentAsByteArray(response.Content,
                cancellationToken);
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

    private async Task<string> TrySendRequest(
        Func<Task<HttpResponseMessage>> sendRequest,
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response =
                await sendRequest().ConfigureAwait(false);

            var content = await response.Content
                .ReadAsStringAsync().ConfigureAwait(false);

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

    private string TrySendRequest(
        Func<HttpResponseMessage> sendRequest,
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = sendRequest();
            var content = ReadContentAsString(response.Content,
                cancellationToken);

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

    private static string ReadContentAsString(HttpContent content,
        CancellationToken cancellationToken = default)
    {
        using Stream stream = content.ReadAsStreamAsync()
            .GetAwaiter()
            .GetResult();
        using var reader = new StreamReader(stream, Encoding.UTF8, true, -1,
            false);

        return reader.ReadToEnd();
    }

    private static byte[] ReadContentAsByteArray(HttpContent content,
        CancellationToken cancellationToken = default)
    {
        using Stream stream = content.ReadAsStreamAsync()
            .GetAwaiter()
            .GetResult();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);

        return memory.ToArray();
    }
}
