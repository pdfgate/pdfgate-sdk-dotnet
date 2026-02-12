namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the GetFile endpoint.
/// </summary>
public sealed record GetFileRequest
{
    /// <summary>
    ///     Existing stored PDF document ID. Provide this or <see cref="File" />.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }
}
