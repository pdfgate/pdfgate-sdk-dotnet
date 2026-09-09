namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the create embed link endpoint.
/// </summary>
public sealed record CreateEmbedLinkRequest
{
    /// <summary>
    ///     Identifier of the envelope to create the embed link for.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }

    /// <summary>
    ///     Identifier of the source document inside the envelope.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Identifier of the embedded recipient the link is for.
    /// </summary>
    public required string RecipientId
    {
        get;
        init;
    }

    /// <summary>
    ///     URL the signing iframe redirects to when the session ends.
    /// </summary>
    public required string ReturnUrl
    {
        get;
        init;
    }
}
