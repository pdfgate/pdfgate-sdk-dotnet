namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the extract PDF form data endpoint.
/// </summary>
public sealed record ExtractPdfFormDataRequest
{
    /// <summary>
    ///     Existing stored PDF document ID.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }
}
