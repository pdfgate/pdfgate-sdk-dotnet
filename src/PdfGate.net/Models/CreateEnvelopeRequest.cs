namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the create envelope endpoint.
/// </summary>
public sealed record CreateEnvelopeRequest
{
    /// <summary>
    ///     Documents to include in the envelope.
    /// </summary>
    public IReadOnlyList<EnvelopeDocument> Documents
    {
        get;
        init;
    } = [];

    /// <summary>
    ///     Name of the requester creating the envelope.
    /// </summary>
    public string RequesterName
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Custom metadata attached to the envelope.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }

    /// <summary>
    ///     Days until the envelope and its signing links expire, counted from
    ///     creation (min 1, max 90). Defaults to the account's envelope
    ///     expiration setting.
    /// </summary>
    public int? ExpiresInDays
    {
        get;
        init;
    }
}

/// <summary>
///     Document included in a create envelope request.
/// </summary>
public sealed record EnvelopeDocument
{
    /// <summary>
    ///     Identifier of the source document.
    /// </summary>
    public string SourceDocumentId
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Display name of the document inside the envelope.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Recipients assigned to the document.
    /// </summary>
    public IReadOnlyList<EnvelopeRecipient> Recipients
    {
        get;
        init;
    } = [];
}

/// <summary>
///     Recipient included in a create envelope request.
///     Provide either <see cref="Email" /> and <see cref="Name" /> for an
///     inline recipient, or <see cref="RecipientId" /> to reuse a stored
///     recipient — never both.
/// </summary>
public sealed record EnvelopeRecipient
{
    /// <summary>
    ///     Recipient email address. Required unless
    ///     <see cref="RecipientId" /> is provided.
    /// </summary>
    public string? Email
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipient display name. Required unless
    ///     <see cref="RecipientId" /> is provided.
    /// </summary>
    public string? Name
    {
        get;
        init;
    }

    /// <summary>
    ///     Identifier of a stored recipient to reuse. Mutually exclusive with
    ///     <see cref="Email" /> and <see cref="Name" />.
    /// </summary>
    public string? RecipientId
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the recipient signs embedded inside your own application.
    ///     Embedded recipients receive no emails from PDFGate; get their
    ///     signing links via the create embed link endpoint after sending.
    /// </summary>
    public bool? Embedded
    {
        get;
        init;
    }

    /// <summary>
    ///     Signing order of the recipient, starting from 1. Recipients sign
    ///     one after another in this order and a recipient is activated once
    ///     everyone with a lower value has signed. Recipients with the same
    ///     value can sign in parallel. Provide it for every recipient of a
    ///     document or for none. Omitted, all recipients can sign immediately.
    /// </summary>
    public int? SigningOrder
    {
        get;
        init;
    }

    /// <summary>
    ///     Optional recipient role.
    /// </summary>
    public string? Role
    {
        get;
        init;
    }

    /// <summary>
    ///     Number of days between signing reminders.
    /// </summary>
    public int? ReminderIntervalDays
    {
        get;
        init;
    }

    /// <summary>
    ///     Maximum number of reminder attempts.
    /// </summary>
    public int? ReminderAttempts
    {
        get;
        init;
    }
}
