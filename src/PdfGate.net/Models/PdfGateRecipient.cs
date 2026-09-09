using System.Text.Json;

namespace PdfGate.net.Models;

/// <summary>
///     Stored recipient metadata returned by the recipient endpoints.
/// </summary>
public sealed record PdfGateRecipient
{
    /// <summary>
    ///     Recipient identifier.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Recipient email address, stored lowercased.
    /// </summary>
    public string Email
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Recipient display name.
    /// </summary>
    public string? Name
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the recipient.
    /// </summary>
    public JsonElement? Metadata
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipient creation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipient last update timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     When the recipient was last used in an envelope, if ever.
    /// </summary>
    public DateTimeOffset? LastUsedAt
    {
        get;
        init;
    }
}

/// <summary>
///     List of stored recipients returned by the list recipients endpoint,
///     ordered oldest first.
/// </summary>
public sealed record PdfGateRecipientList
{
    /// <summary>
    ///     Stored recipients matching the lookup email.
    /// </summary>
    public IReadOnlyList<PdfGateRecipient> Recipients
    {
        get;
        init;
    } = [];
}
