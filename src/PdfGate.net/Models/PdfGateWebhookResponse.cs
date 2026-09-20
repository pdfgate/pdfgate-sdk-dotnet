using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Webhook metadata returned by the PDFGate API.
/// </summary>
public sealed record PdfGateWebhookResponse
{
    /// <summary>
    ///     Webhook identifier.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Webhook endpoint URL.
    /// </summary>
    public string Url
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Events the webhook is subscribed to.
    /// </summary>
    [JsonConverter(typeof(WebhookEventTypeListJsonConverter))]
    public IReadOnlyList<WebhookEventType> EventTypes
    {
        get;
        init;
    } = [];

    /// <summary>
    ///     Webhook status.
    /// </summary>
    [JsonConverter(typeof(NullableWebhookStatusJsonConverter))]
    public WebhookStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     Webhook description.
    /// </summary>
    public string? Description
    {
        get;
        init;
    }

    /// <summary>
    ///     Signing secret. Only returned once, when the webhook is created.
    /// </summary>
    public string? Secret
    {
        get;
        init;
    }

    /// <summary>
    ///     Creation timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Last update timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt
    {
        get;
        init;
    }
}

/// <summary>
///     Webhook status values returned by the API.
/// </summary>
[JsonConverter(typeof(WebhookStatusJsonConverter))]
public enum WebhookStatus
{
    /// <summary>
    ///     The webhook is active and receiving events.
    /// </summary>
    Active,

    /// <summary>
    ///     The webhook is disabled and not receiving events.
    /// </summary>
    Disabled
}

/// <summary>
///     Events that a webhook can subscribe to.
/// </summary>
[JsonConverter(typeof(WebhookEventTypeJsonConverter))]
public enum WebhookEventType
{
    /// <summary>
    ///     An envelope was sent to its recipients.
    /// </summary>
    EnvelopeSent,

    /// <summary>
    ///     An envelope was completed by all recipients.
    /// </summary>
    EnvelopeCompleted,

    /// <summary>
    ///     An envelope expired before completion.
    /// </summary>
    EnvelopeExpired,

    /// <summary>
    ///     An envelope was voided (cancelled) by the sender.
    /// </summary>
    EnvelopeVoided,

    /// <summary>
    ///     An envelope was permanently deleted by the sender.
    /// </summary>
    EnvelopeDeleted,

    /// <summary>
    ///     It became a recipient's turn to sign on a document with a signing
    ///     order. Fires for every recipient, including those activated when
    ///     the envelope is sent.
    /// </summary>
    EnvelopeRecipientActivated,

    /// <summary>
    ///     A recipient signed a document within an envelope.
    /// </summary>
    EnvelopeRecipientSigned,

    /// <summary>
    ///     A document within an envelope was completed.
    /// </summary>
    EnvelopeDocumentCompleted
}

internal sealed class WebhookStatusJsonConverter
    : JsonConverter<WebhookStatus>
{
    public override WebhookStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "active" => WebhookStatus.Active,
            "disabled" => WebhookStatus.Disabled,
            _ => throw new JsonException($"Unknown webhook status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, WebhookStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            WebhookStatus.Active => "active",
            WebhookStatus.Disabled => "disabled",
            _ => throw new JsonException(
                $"Unknown webhook status value: '{value}'.")
        });
    }
}

internal sealed class NullableWebhookStatusJsonConverter
    : NullableStructJsonConverter<WebhookStatus>
{
    protected override JsonConverter<WebhookStatus> InnerConverter
    {
        get;
    } = new WebhookStatusJsonConverter();
}

/// <summary>
///     Converts a list of <see cref="WebhookEventType" /> using the element converter,
///     bypassing the globally-registered enum converter.
/// </summary>
internal sealed class WebhookEventTypeListJsonConverter
    : JsonConverter<IReadOnlyList<WebhookEventType>>
{
    public override IReadOnlyList<WebhookEventType> Read(
        ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected a JSON array of webhook events.");

        var results = new List<WebhookEventType>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return results;

            // Forward compatibility: unrecognized event types (e.g. newly added ones)
            // are skipped instead of failing the whole response.
            WebhookEventType? parsed =
                WebhookEventTypeJsonConverter.TryFromWire(reader.GetString());
            if (parsed.HasValue)
                results.Add(parsed.Value);
        }

        throw new JsonException("Unexpected end of JSON array.");
    }

    public override void Write(Utf8JsonWriter writer,
        IReadOnlyList<WebhookEventType> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (WebhookEventType item in value)
            writer.WriteStringValue(WebhookEventTypeJsonConverter.ToWire(item));

        writer.WriteEndArray();
    }
}

internal sealed class WebhookEventTypeJsonConverter
    : JsonConverter<WebhookEventType>
{
    public static WebhookEventType? TryFromWire(string? value)
    {
        return value switch
        {
            "envelope.sent" => WebhookEventType.EnvelopeSent,
            "envelope.completed" => WebhookEventType.EnvelopeCompleted,
            "envelope.expired" => WebhookEventType.EnvelopeExpired,
            "envelope.voided" => WebhookEventType.EnvelopeVoided,
            "envelope.deleted" => WebhookEventType.EnvelopeDeleted,
            "envelope.recipient.activated" =>
                WebhookEventType.EnvelopeRecipientActivated,
            "envelope.recipient.signed" =>
                WebhookEventType.EnvelopeRecipientSigned,
            "envelope.document.completed" =>
                WebhookEventType.EnvelopeDocumentCompleted,
            _ => (WebhookEventType?)null
        };
    }

    public static string ToWire(WebhookEventType value)
    {
        return value switch
        {
            WebhookEventType.EnvelopeSent => "envelope.sent",
            WebhookEventType.EnvelopeCompleted => "envelope.completed",
            WebhookEventType.EnvelopeExpired => "envelope.expired",
            WebhookEventType.EnvelopeVoided => "envelope.voided",
            WebhookEventType.EnvelopeDeleted => "envelope.deleted",
            WebhookEventType.EnvelopeRecipientActivated =>
                "envelope.recipient.activated",
            WebhookEventType.EnvelopeRecipientSigned =>
                "envelope.recipient.signed",
            WebhookEventType.EnvelopeDocumentCompleted =>
                "envelope.document.completed",
            _ => throw new JsonException(
                $"Unknown webhook event type value: '{value}'.")
        };
    }

    public override WebhookEventType Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        WebhookEventType? parsed = TryFromWire(reader.GetString());
        if (parsed.HasValue)
            return parsed.Value;

        throw new JsonException("Unknown webhook event type.");
    }

    public override void Write(Utf8JsonWriter writer, WebhookEventType value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToWire(value));
    }
}
