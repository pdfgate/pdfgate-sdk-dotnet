namespace PdfGate.net.Models;

/// <summary>
///     Request payload used to create a stored recipient.
/// </summary>
/// <remarks>
///     Emails are not unique: every call creates a new recipient, even when a
///     recipient with the same email already exists.
/// </remarks>
public sealed record CreateRecipientRequest
{
    /// <summary>
    ///     Recipient email address. Stored lowercased and cannot be changed
    ///     after creation.
    /// </summary>
    public required string Email
    {
        get;
        init;
    }

    /// <summary>
    ///     Optional recipient display name.
    /// </summary>
    public string? Name
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the recipient.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}

/// <summary>
///     Request payload used to list stored recipients by email.
/// </summary>
public sealed record ListRecipientsRequest
{
    /// <summary>
    ///     Email address to look up. The lookup is case-insensitive.
    /// </summary>
    public required string Email
    {
        get;
        init;
    }
}

/// <summary>
///     Request payload used to fetch a stored recipient by ID.
/// </summary>
public sealed record GetRecipientRequest
{
    /// <summary>
    ///     Recipient identifier.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }
}

/// <summary>
///     Request payload used to update a stored recipient.
/// </summary>
/// <remarks>
///     The email address cannot be changed. Existing envelopes are not
///     affected: they keep the recipient name they were created with.
/// </remarks>
public sealed record UpdateRecipientRequest
{
    /// <summary>
    ///     Recipient identifier.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }

    /// <summary>
    ///     New recipient display name.
    /// </summary>
    public string? Name
    {
        get;
        init;
    }

    /// <summary>
    ///     New custom metadata attached to the recipient.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}
