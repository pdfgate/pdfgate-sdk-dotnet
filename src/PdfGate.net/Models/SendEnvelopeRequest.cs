namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the send envelope endpoint.
/// </summary>
public sealed record SendEnvelopeRequest
{
    /// <summary>
    ///     Identifier of the envelope to send.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;
}
