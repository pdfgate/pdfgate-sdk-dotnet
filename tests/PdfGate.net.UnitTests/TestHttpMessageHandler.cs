namespace PdfGate.net.UnitTests;

internal sealed class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken,
        Task<HttpResponseMessage>> _sendAsync;
    private readonly Func<HttpRequestMessage, CancellationToken,
        HttpResponseMessage> _send;

    public TestHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> send)
        : this((request, cancellationToken) =>
                Task.FromResult(send(request, cancellationToken)),
            send)
    {
    }

    public TestHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
        : this((request, _) => sendAsync(request),
            (request, _) => sendAsync(request).GetAwaiter().GetResult())
    {
    }

    public TestHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>
            sendAsync)
        : this(sendAsync,
            (request, cancellationToken) =>
                sendAsync(request, cancellationToken).GetAwaiter().GetResult())
    {
    }

    public TestHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>
            sendAsync,
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> send)
    {
        _sendAsync = sendAsync;
        _send = send;
    }

    public CancellationToken LastCancellationToken
    {
        get;
        private set;
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;
        return _send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastCancellationToken = cancellationToken;
        return _sendAsync(request, cancellationToken);
    }
}
