using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Request payload used to register a webhook endpoint.
/// </summary>
/// <remarks>
///     The webhook URL must be publicly accessible (localhost is not supported). The response
///     includes a secret that is only returned once, at creation time.
/// </remarks>
public sealed record CreateWebhookRequest
{
    /// <summary>
    ///     Webhook endpoint URL.
    /// </summary>
    public required string Url
    {
        get;
        init;
    }

    /// <summary>
    ///     Events to subscribe to.
    /// </summary>
    [JsonConverter(typeof(WebhookEventTypeListJsonConverter))]
    public required IReadOnlyList<WebhookEventType> EventTypes
    {
        get;
        init;
    }

    /// <summary>
    ///     Optional webhook description.
    /// </summary>
    public string? Description
    {
        get;
        init;
    }
}

/// <summary>
///     Request payload used to fetch a webhook by ID.
/// </summary>
public sealed record GetWebhookRequest
{
    /// <summary>
    ///     Webhook identifier.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }
}

/// <summary>
///     Request payload used to delete a webhook by ID.
/// </summary>
public sealed record DeleteWebhookRequest
{
    /// <summary>
    ///     Webhook identifier.
    /// </summary>
    public required string Id
    {
        get;
        init;
    }
}
