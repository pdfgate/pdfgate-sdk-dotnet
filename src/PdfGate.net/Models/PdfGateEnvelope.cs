using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Envelope metadata returned by the create envelope endpoint.
/// </summary>
public sealed record PdfGateEnvelope
{
    /// <summary>
    ///     Envelope identifier.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Envelope status.
    /// </summary>
    public EnvelopeStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     Documents in the envelope.
    /// </summary>
    public IReadOnlyList<EnvelopeDocumentResponse> Documents
    {
        get;
        init;
    } = [];

    /// <summary>
    ///     Envelope creation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Envelope completion timestamp.
    /// </summary>
    public DateTimeOffset? CompletedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Envelope expiration timestamp.
    /// </summary>
    public DateTimeOffset? ExpiredAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the envelope.
    /// </summary>
    public JsonElement? Metadata
    {
        get;
        init;
    }
}

/// <summary>
///     Envelope document metadata returned by the API.
/// </summary>
public sealed record EnvelopeDocumentResponse
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
    ///     Identifier of the signed document, when available.
    /// </summary>
    public string? SignedDocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipients assigned to the document.
    /// </summary>
    public IReadOnlyList<EnvelopeRecipientResponse> Recipients
    {
        get;
        init;
    } = [];

    /// <summary>
    ///     Envelope document status.
    /// </summary>
    public EnvelopeDocumentStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     Completion timestamp for the document.
    /// </summary>
    public DateTimeOffset? CompletedAt
    {
        get;
        init;
    }
}

/// <summary>
///     Envelope recipient metadata returned by the API.
/// </summary>
public sealed record EnvelopeRecipientResponse
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
    ///     Recipient status.
    /// </summary>
    public DocumentRecipientStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     Timestamp at which the recipient signed the document.
    /// </summary>
    public DateTimeOffset? SignedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Timestamp at which the recipient viewed the document.
    /// </summary>
    public DateTimeOffset? ViewedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Fields available to the recipient.
    /// </summary>
    public IReadOnlyList<EnvelopeFieldResponse> Fields
    {
        get;
        init;
    } = [];
}

/// <summary>
///     Envelope field metadata returned by the API.
/// </summary>
public sealed record EnvelopeFieldResponse
{
    /// <summary>
    ///     Field name.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Field type.
    /// </summary>
    public DocumentFieldType? Type
    {
        get;
        init;
    }

    /// <summary>
    ///     Field value.
    /// </summary>
    public JsonElement? Value
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the field is checked.
    /// </summary>
    public bool? Checked
    {
        get;
        init;
    }
}

/// <summary>
///     Status values returned by the API for envelope processing.
/// </summary>
[JsonConverter(typeof(EnvelopeStatusJsonConverter))]
public enum EnvelopeStatus
{
    /// <summary>
    ///     The envelope has been created.
    /// </summary>
    Created,

    /// <summary>
    ///     The envelope is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    ///     The envelope has been completed.
    /// </summary>
    Completed,

    /// <summary>
    ///     The envelope has expired.
    /// </summary>
    Expired
}

/// <summary>
///     Status values returned by the API for envelope documents.
/// </summary>
[JsonConverter(typeof(EnvelopeDocumentStatusJsonConverter))]
public enum EnvelopeDocumentStatus
{
    /// <summary>
    ///     The document is pending.
    /// </summary>
    Pending,

    /// <summary>
    ///     The document has been sent for signing.
    /// </summary>
    SentForSigning,

    /// <summary>
    ///     Signing is currently in progress.
    /// </summary>
    SigningInProgress,

    /// <summary>
    ///     Signing failed.
    /// </summary>
    SigningFailed,

    /// <summary>
    ///     The document has been completed.
    /// </summary>
    Completed
}

/// <summary>
///     Status values returned by the API for envelope recipients.
/// </summary>
[JsonConverter(typeof(DocumentRecipientStatusJsonConverter))]
public enum DocumentRecipientStatus
{
    /// <summary>
    ///     The recipient has not signed yet.
    /// </summary>
    Pending,

    /// <summary>
    ///     The recipient has signed.
    /// </summary>
    Signed
}

/// <summary>
///     Field types returned by the API for envelope fields.
/// </summary>
[JsonConverter(typeof(DocumentFieldTypeJsonConverter))]
public enum DocumentFieldType
{
    /// <summary>
    ///     Signature field.
    /// </summary>
    Signature,

    /// <summary>
    ///     Text field.
    /// </summary>
    Text,

    /// <summary>
    ///     Number field.
    /// </summary>
    Number,

    /// <summary>
    ///     Text area field.
    /// </summary>
    TextArea,

    /// <summary>
    ///     Date field.
    /// </summary>
    Date,

    /// <summary>
    ///     Time field.
    /// </summary>
    Time,

    /// <summary>
    ///     Date/time field.
    /// </summary>
    Datetime,

    /// <summary>
    ///     Checkbox field.
    /// </summary>
    Checkbox,

    /// <summary>
    ///     Radio button field.
    /// </summary>
    RadioButton,

    /// <summary>
    ///     Select field.
    /// </summary>
    Select
}

internal sealed class EnvelopeStatusJsonConverter
    : JsonConverter<EnvelopeStatus>
{
    public override EnvelopeStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "created" => EnvelopeStatus.Created,
            "in_progress" => EnvelopeStatus.InProgress,
            "completed" => EnvelopeStatus.Completed,
            "expired" => EnvelopeStatus.Expired,
            _ => throw new JsonException($"Unknown envelope status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, EnvelopeStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            EnvelopeStatus.Created => "created",
            EnvelopeStatus.InProgress => "in_progress",
            EnvelopeStatus.Completed => "completed",
            EnvelopeStatus.Expired => "expired",
            _ => throw new JsonException(
                $"Unknown envelope status value: '{value}'.")
        });
    }
}

internal sealed class EnvelopeDocumentStatusJsonConverter
    : JsonConverter<EnvelopeDocumentStatus>
{
    public override EnvelopeDocumentStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => EnvelopeDocumentStatus.Pending,
            "sent_for_signing" => EnvelopeDocumentStatus.SentForSigning,
            "signing_in_progress" => EnvelopeDocumentStatus.SigningInProgress,
            "signing_failed" => EnvelopeDocumentStatus.SigningFailed,
            "completed" => EnvelopeDocumentStatus.Completed,
            _ => throw new JsonException(
                $"Unknown envelope document status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer,
        EnvelopeDocumentStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            EnvelopeDocumentStatus.Pending => "pending",
            EnvelopeDocumentStatus.SentForSigning => "sent_for_signing",
            EnvelopeDocumentStatus.SigningInProgress => "signing_in_progress",
            EnvelopeDocumentStatus.SigningFailed => "signing_failed",
            EnvelopeDocumentStatus.Completed => "completed",
            _ => throw new JsonException(
                $"Unknown envelope document status value: '{value}'.")
        });
    }
}

internal sealed class DocumentRecipientStatusJsonConverter
    : JsonConverter<DocumentRecipientStatus>
{
    public override DocumentRecipientStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => DocumentRecipientStatus.Pending,
            "signed" => DocumentRecipientStatus.Signed,
            _ => throw new JsonException(
                $"Unknown document recipient status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer,
        DocumentRecipientStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            DocumentRecipientStatus.Pending => "pending",
            DocumentRecipientStatus.Signed => "signed",
            _ => throw new JsonException(
                $"Unknown document recipient status value: '{value}'.")
        });
    }
}

internal sealed class DocumentFieldTypeJsonConverter
    : JsonConverter<DocumentFieldType>
{
    public override DocumentFieldType Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "signature" => DocumentFieldType.Signature,
            "text" => DocumentFieldType.Text,
            "number" => DocumentFieldType.Number,
            "textarea" => DocumentFieldType.TextArea,
            "date" => DocumentFieldType.Date,
            "time" => DocumentFieldType.Time,
            "datetime" => DocumentFieldType.Datetime,
            "checkbox" => DocumentFieldType.Checkbox,
            "radio" => DocumentFieldType.RadioButton,
            "select" => DocumentFieldType.Select,
            _ => throw new JsonException(
                $"Unknown document field type: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, DocumentFieldType value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            DocumentFieldType.Signature => "signature",
            DocumentFieldType.Text => "text",
            DocumentFieldType.Number => "number",
            DocumentFieldType.TextArea => "textarea",
            DocumentFieldType.Date => "date",
            DocumentFieldType.Time => "time",
            DocumentFieldType.Datetime => "datetime",
            DocumentFieldType.Checkbox => "checkbox",
            DocumentFieldType.RadioButton => "radio",
            DocumentFieldType.Select => "select",
            _ => throw new JsonException(
                $"Unknown document field type value: '{value}'.")
        });
    }
}
