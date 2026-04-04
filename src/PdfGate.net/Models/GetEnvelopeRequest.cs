namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the get envelope endpoint.
/// </summary>
public sealed record GetEnvelopeRequest
{
    /// <summary>
    ///     Identifier of the envelope to retrieve.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;
}
