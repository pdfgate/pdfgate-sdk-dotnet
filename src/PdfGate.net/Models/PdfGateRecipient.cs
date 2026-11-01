using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>
    ///     Automatic signing. The stored signature of the recipient or the
    ///     state of its enrolment. Null until an enrolment was requested.
    /// </summary>
    public PdfGateRecipientStoredSignature? Signature
    {
        get;
        init;
    }
}

/// <summary>
///     Automatic signing. The reusable signature a recipient stored through
///     the enrolment link, or the state of the enrolment.
/// </summary>
public sealed record PdfGateRecipientStoredSignature
{
    /// <summary>
    ///     State of the stored signature.
    /// </summary>
    [JsonConverter(typeof(NullableStoredSignatureStatusJsonConverter))]
    public StoredSignatureStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     When the enrolment link was last sent.
    /// </summary>
    public DateTimeOffset? RequestedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     How the recipient created the signature.
    /// </summary>
    [JsonConverter(typeof(NullableSignatureAdoptionMethodJsonConverter))]
    public SignatureAdoptionMethod? AdoptionMethod
    {
        get;
        init;
    }

    /// <summary>
    ///     When the recipient completed the enrolment.
    /// </summary>
    public DateTimeOffset? EnrolledAt
    {
        get;
        init;
    }

    /// <summary>
    ///     When the signature was removed.
    /// </summary>
    public DateTimeOffset? RevokedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     When the signature was last applied to a document.
    /// </summary>
    public DateTimeOffset? LastUsedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     How many documents the signature was applied to.
    /// </summary>
    public int UseCount
    {
        get;
        init;
    }

    /// <summary>
    ///     The signature as a PNG data URL. Returned only by the get
    ///     recipient endpoint while the signature is active, never in the
    ///     list.
    /// </summary>
    public string? Image
    {
        get;
        init;
    }
}

/// <summary>
///     State of a recipient's stored signature (automatic signing).
/// </summary>
[JsonConverter(typeof(StoredSignatureStatusJsonConverter))]
public enum StoredSignatureStatus
{
    /// <summary>
    ///     The enrolment link was sent but the recipient has not completed
    ///     it.
    /// </summary>
    Pending,

    /// <summary>
    ///     The signature can be applied to documents.
    /// </summary>
    Active,

    /// <summary>
    ///     The signature was removed.
    /// </summary>
    Revoked
}

/// <summary>
///     How a recipient created their stored signature.
/// </summary>
[JsonConverter(typeof(SignatureAdoptionMethodJsonConverter))]
public enum SignatureAdoptionMethod
{
    /// <summary>
    ///     The recipient typed their signature.
    /// </summary>
    TypeSignature,

    /// <summary>
    ///     The recipient drew their signature.
    /// </summary>
    DrawSignature,

    /// <summary>
    ///     The recipient uploaded an image of their signature.
    /// </summary>
    UploadSignature
}

internal sealed class StoredSignatureStatusJsonConverter
    : JsonConverter<StoredSignatureStatus>
{
    public override StoredSignatureStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => StoredSignatureStatus.Pending,
            "active" => StoredSignatureStatus.Active,
            "revoked" => StoredSignatureStatus.Revoked,
            _ => throw new JsonException(
                $"Unknown stored signature status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer,
        StoredSignatureStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            StoredSignatureStatus.Pending => "pending",
            StoredSignatureStatus.Active => "active",
            StoredSignatureStatus.Revoked => "revoked",
            _ => throw new JsonException(
                $"Unknown stored signature status value: '{value}'.")
        });
    }
}

internal sealed class NullableStoredSignatureStatusJsonConverter
    : NullableStructJsonConverter<StoredSignatureStatus>
{
    protected override JsonConverter<StoredSignatureStatus> InnerConverter
    {
        get;
    } = new StoredSignatureStatusJsonConverter();
}

internal sealed class SignatureAdoptionMethodJsonConverter
    : JsonConverter<SignatureAdoptionMethod>
{
    public override SignatureAdoptionMethod Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "type_signature" => SignatureAdoptionMethod.TypeSignature,
            "draw_signature" => SignatureAdoptionMethod.DrawSignature,
            "upload_signature" => SignatureAdoptionMethod.UploadSignature,
            _ => throw new JsonException(
                $"Unknown signature adoption method: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer,
        SignatureAdoptionMethod value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            SignatureAdoptionMethod.TypeSignature => "type_signature",
            SignatureAdoptionMethod.DrawSignature => "draw_signature",
            SignatureAdoptionMethod.UploadSignature => "upload_signature",
            _ => throw new JsonException(
                $"Unknown signature adoption method value: '{value}'.")
        });
    }
}

internal sealed class NullableSignatureAdoptionMethodJsonConverter
    : NullableStructJsonConverter<SignatureAdoptionMethod>
{
    protected override JsonConverter<SignatureAdoptionMethod> InnerConverter
    {
        get;
    } = new SignatureAdoptionMethodJsonConverter();
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
