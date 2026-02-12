namespace PdfGate.net.Models;

public sealed record UploadFileRequest
{
    /// <summary>
    ///     File content bytes.
    /// </summary>
    public Stream Content
    {
        get;
        init;
    } = Stream.Null;

    /// <summary>
    ///     URL to a PDF file
    /// </summary>
    public Uri? Url
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the generated document.
    /// </summary>
    public object? Metadata
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
