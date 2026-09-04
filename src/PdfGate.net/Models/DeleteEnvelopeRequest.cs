namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the delete envelope endpoint.
/// </summary>
public sealed record DeleteEnvelopeRequest
{
    /// <summary>
    ///     Identifier of the envelope to delete.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }
}
