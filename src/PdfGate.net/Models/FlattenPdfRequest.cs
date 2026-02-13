namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the flatten PDF endpoint.
/// </summary>
public sealed record FlattenPdfRequest
{
    /// <summary>
    ///     Existing stored PDF document ID..
    /// </summary>
    public string? DocumentId
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
    ///     Custom metadata attached to the flattened document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}
