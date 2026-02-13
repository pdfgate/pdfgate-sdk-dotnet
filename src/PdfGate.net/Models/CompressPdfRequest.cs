namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the compress PDF endpoint.
/// </summary>
public sealed record CompressPdfRequest
{
    /// <summary>
    ///     Existing stored PDF document ID.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Enables linearized PDF output when true.
    /// </summary>
    public bool? Linearize
    {
        get;
        init;
    }

    /// <summary>
    ///     Always requests JSON metadata responses from the API.
    /// </summary>
    public bool JsonResponse
    {
        get;
    } = true;

    /// <summary>
    ///     Pre-signed URL expiration in seconds.
    /// </summary>
    public long? PreSignedUrlExpiresIn
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the compressed document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}
