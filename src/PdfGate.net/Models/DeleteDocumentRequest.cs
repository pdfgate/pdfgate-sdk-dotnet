namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the delete document endpoint.
/// </summary>
public sealed record DeleteDocumentRequest
{
    /// <summary>
    ///     Existing stored PDF document ID to delete.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }
}
