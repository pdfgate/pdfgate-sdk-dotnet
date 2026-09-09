namespace PdfGate.net.Models;

/// <summary>
///     Embed link metadata returned by the create embed link endpoint.
/// </summary>
public sealed record PdfGateEmbedLink
{
    /// <summary>
    ///     URL to load in an iframe so the recipient can sign inside your
    ///     application.
    /// </summary>
    public string Url
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     When the embed link expires (10 minutes after creation).
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get;
        init;
    }
}
