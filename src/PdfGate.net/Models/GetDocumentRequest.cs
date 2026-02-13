namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the GetDocument endpoint.
/// </summary>
public sealed record GetDocumentRequest
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
    ///     Pre-signed URL expiration in seconds.
    /// </summary>
    public long? PreSignedUrlExpiresIn
    {
        get;
        init;
    }
}
