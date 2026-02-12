using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Document metadata returned by PDFGate JSON endpoints.
/// </summary>
public sealed record PdfGateDocumentResponse
{
    /// <summary>
    ///     Document identifier.
    /// </summary>
    public string Id
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    ///     Processing status.
    /// </summary>
    public DocumentStatus? Status
    {
        get;
        init;
    }

    /// <summary>
    ///     Document type.
    /// </summary>
    [JsonConverter(typeof(NullableDocumentTypeJsonConverter))]
    public DocumentType? Type
    {
        get;
        init;
    }

    /// <summary>
    ///     Pre-signed file URL.
    /// </summary>
    public string? FileUrl
    {
        get;
        init;
    }

    /// <summary>
    ///     File size in bytes.
    /// </summary>
    public long? Size
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
    ///     Date until it will be stored
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get;
        init;
    }

    /// <summary>
    ///     Document ID. This document was the result of modifying derivedFrom.
    /// </summary>
    public string? derivedFrom
    {
        get;
        init;
    } = string.Empty;
}

/// <summary>
///     Status values returned by the API for document processing.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    ///     The document is finished and available.
    /// </summary>
    Completed,

    /// <summary>
    ///     The document is still processing.
    /// </summary>
    Processing,

    /// <summary>
    ///     The document has expired and is no longer available.
    /// </summary>
    Expired,

    /// <summary>
    ///     The document failed to process.
    /// </summary>
    Failed
}

/// <summary>
///     Document type values returned by the API.
/// </summary>
public enum DocumentType
{
    /// <summary>
    ///     Document generated from HTML or URL.
    /// </summary>
    FromHtml,

    /// <summary>
    ///     Document created by flattening a PDF.
    /// </summary>
    Flattened,

    /// <summary>
    ///     Document created by applying a watermark.
    /// </summary>
    Watermarked,

    /// <summary>
    ///     Document created by encryption.
    /// </summary>
    Encrypted,

    /// <summary>
    ///     Document created by compression.
    /// </summary>
    Compressed,

    /// <summary>
    ///     Document created by signing.
    /// </summary>
    Signed
}

internal sealed class
    NullableDocumentTypeJsonConverter : JsonConverter<DocumentType?>
{
    public override DocumentType? Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        return value switch
        {
            "from_html" => DocumentType.FromHtml,
            "flattened" => DocumentType.Flattened,
            "watermarked" => DocumentType.Watermarked,
            "encrypted" => DocumentType.Encrypted,
            "compressed" => DocumentType.Compressed,
            "signed" => DocumentType.Signed,
            _ => throw new JsonException($"Unknown document type: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, DocumentType? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var wireValue = value switch
        {
            DocumentType.FromHtml => "from_html",
            DocumentType.Flattened => "flattened",
            DocumentType.Watermarked => "watermarked",
            DocumentType.Encrypted => "encrypted",
            DocumentType.Compressed => "compressed",
            DocumentType.Signed => "signed",
            _ => throw new JsonException(
                $"Unknown document type value: '{value}'.")
        };

        writer.WriteStringValue(wireValue);
    }
}
