using System.Net;

namespace PdfGate.net;

/// <summary>
/// Represents errors returned by or encountered while calling the PDFGate API.
/// </summary>
public sealed class PdfGateException : Exception
{
    private const int MaxBodyLength = 1024;

    /// <summary>
    /// Creates an instance with a message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public PdfGateException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an instance with a message and underlying cause.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Underlying exception.</param>
    public PdfGateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// HTTP status code when available.
    /// </summary>
    public HttpStatusCode? StatusCode
    {
        get; init;
    }

    /// <summary>
    /// HTTP response body when available.
    /// </summary>
    public string? ResponseBody
    {
        get; init;
    }

    /// <summary>
    /// Creates an exception from an HTTP error response.
    /// </summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="endpoint">Called endpoint path.</param>
    /// <param name="body">Response body content.</param>
    /// <returns>A populated <see cref="PdfGateException"/> instance.</returns>
    public static PdfGateException FromHttpError(HttpStatusCode statusCode, string endpoint, string body)
    {
        var truncatedBody = body.Length > MaxBodyLength
            ? body.Substring(0, MaxBodyLength)
            : body;

        var message = $"PDFGate request to '{endpoint}' failed with status code {(int)statusCode} ({statusCode}). Response body: {truncatedBody}";

        return new PdfGateException(message)
        {
            StatusCode = statusCode,
            ResponseBody = truncatedBody
        };
    }
}
