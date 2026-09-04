namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the void envelope endpoint.
/// </summary>
public sealed record VoidEnvelopeRequest
{
    /// <summary>
    ///     Identifier of the envelope to void.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }

    /// <summary>
    ///     Optional reason for voiding (max 500 characters). Visible to
    ///     recipients: included in the cancellation email sent to recipients
    ///     who had not signed yet.
    /// </summary>
    public string? Reason
    {
        get;
        init;
    }
}
