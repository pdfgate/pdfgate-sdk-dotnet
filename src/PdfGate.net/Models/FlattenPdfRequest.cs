namespace PdfGate.net.Models;

/// <summary>
///     File payload used by PDFGate multipart endpoints.
/// </summary>
public sealed record PdfGateFile
{
    /// <summary>
    ///     File name sent to the API.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = "input.pdf";

    /// <summary>
    ///     File content bytes.
    /// </summary>
    public byte[] Content
    {
        get;
        init;
    } = [];

    /// <summary>
    ///     File MIME type.
    /// </summary>
    public string Type
    {
        get;
        init;
    } = "application/pdf";
}

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
