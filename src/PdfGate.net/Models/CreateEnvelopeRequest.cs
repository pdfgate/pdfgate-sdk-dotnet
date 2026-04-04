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
/// </summary>
public sealed record EnvelopeRecipient
{
    /// <summary>
    ///     Recipient email address.
    /// </summary>
    public string Email
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Recipient display name.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Optional recipient role.
    /// </summary>
    public string? Role
    {
        get;
        init;
    }
}
