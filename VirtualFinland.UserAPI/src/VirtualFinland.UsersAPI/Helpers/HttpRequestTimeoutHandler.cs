using VirtualFinland.UserAPI.Helpers.Extensions;

namespace VirtualFinland.UserAPI.Helpers;

/// <summary>
/// Delegating handler that will throw a <see cref="TimeoutException" /> if the request times out.
/// The timeout is determined by the <see cref="HttpRequestMessage.GetTimeout" /> extension method.
/// If no timeout is specified, the <see cref="DefaultTimeout" /> will be used.
/// </summary>
/// <remarks>
/// Inspired by Thomas Levesque article "Better timeout handling with HttpClient" (February 25, 2018)
///  - https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
/// </remarks>
public class HttpRequestTimeoutHandler : DelegatingHandler
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);
    public string DefaultTimeoutMessage { get; set; } = "Request timeout";

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var cts = GetCancellationTokenSource(request, cancellationToken);
        try
        {
            return await base.SendAsync(request, cts?.Token ?? cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(DefaultTimeoutMessage);
        }
    }

    private CancellationTokenSource? GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timeout = request.GetTimeout() ?? DefaultTimeout;
        if (timeout == Timeout.InfiniteTimeSpan)
        {
            // No need to create a CTS if there's no timeout
            return null;
        }
        else
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            return cts;
        }
    }
}