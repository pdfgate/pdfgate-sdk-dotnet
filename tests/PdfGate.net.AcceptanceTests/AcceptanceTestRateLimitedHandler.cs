namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Delegating handler used by acceptance tests to limit request concurrency and request rate.
/// </summary>
internal sealed class AcceptanceTestRateLimitedHandler : DelegatingHandler
{
    private readonly object _rateLimitLock = new();
    private readonly Queue<DateTimeOffset> _requestTimestamps = [];
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly int _maxRequestsPerSecond;

    public AcceptanceTestRateLimitedHandler(HttpMessageHandler innerHandler,
        int maxConcurrency = 2,
        int maxRequestsPerSecond = 2)
        : base(innerHandler)
    {
        _concurrencySemaphore = new SemaphoreSlim(maxConcurrency,
            maxConcurrency);
        _maxRequestsPerSecond = maxRequestsPerSecond;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await WaitForRateLimitAsync(cancellationToken).ConfigureAwait(false);
        await _concurrencySemaphore.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            return await base.SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        WaitForRateLimit(cancellationToken);
        _concurrencySemaphore.Wait(cancellationToken);
        try
        {
            return base.Send(request, cancellationToken);
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    private async Task WaitForRateLimitAsync(
        CancellationToken cancellationToken)
    {
        while (true)
        {
            TimeSpan delay;

            lock (_rateLimitLock)
            {
                var now = DateTimeOffset.UtcNow;
                TrimOldRequestTimestamps(now);

                if (_requestTimestamps.Count < _maxRequestsPerSecond)
                {
                    _requestTimestamps.Enqueue(now);
                    return;
                }

                var oldest = _requestTimestamps.Peek();
                delay = oldest.AddSeconds(1) - now;
            }

            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private void WaitForRateLimit(CancellationToken cancellationToken)
    {
        while (true)
        {
            TimeSpan delay;

            lock (_rateLimitLock)
            {
                var now = DateTimeOffset.UtcNow;
                TrimOldRequestTimestamps(now);

                if (_requestTimestamps.Count < _maxRequestsPerSecond)
                {
                    _requestTimestamps.Enqueue(now);
                    return;
                }

                var oldest = _requestTimestamps.Peek();
                delay = oldest.AddSeconds(1) - now;
            }

            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            cancellationToken.ThrowIfCancellationRequested();
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.WaitHandle.WaitOne(delay);
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                Thread.Sleep(delay);
            }
        }
    }

    private void TrimOldRequestTimestamps(DateTimeOffset now)
    {
        while (_requestTimestamps.Count > 0)
        {
            var oldest = _requestTimestamps.Peek();
            if (now - oldest < TimeSpan.FromSeconds(1))
                break;

            _requestTimestamps.Dequeue();
        }
    }
}
