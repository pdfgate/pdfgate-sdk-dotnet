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
    [JsonConverter(typeof(NullableDocumentStatusJsonConverter))]
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
    public string? DerivedFrom
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
    Signed,

    /// <summary>
    ///     Document uploaded by file or URL.
    /// </summary>
    Uploaded,

    /// <summary>
    ///     Signature audit log document produced by a signing flow.
    /// </summary>
    SignatureAuditLog,

    /// <summary>
    ///     Document created by adding form fields.
    /// </summary>
    DocumentFieldsAdded,

    /// <summary>
    ///     Signing template document.
    /// </summary>
    SigningTemplate
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
            "uploaded" => DocumentType.Uploaded,
            "signature_audit_log" => DocumentType.SignatureAuditLog,
            "document_fields_added" => DocumentType.DocumentFieldsAdded,
            "signing_template" => DocumentType.SigningTemplate,
            // Forward compatibility: unrecognized values are surfaced as null.
            _ => (DocumentType?)null
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
            DocumentType.Uploaded => "uploaded",
            DocumentType.SignatureAuditLog => "signature_audit_log",
            DocumentType.DocumentFieldsAdded => "document_fields_added",
            DocumentType.SigningTemplate => "signing_template",
            _ => throw new JsonException(
                $"Unknown document type value: '{value}'.")
        };

        writer.WriteStringValue(wireValue);
    }
}

internal sealed class DocumentStatusJsonConverter
    : JsonConverter<DocumentStatus>
{
    public override DocumentStatus Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "completed" => DocumentStatus.Completed,
            "processing" => DocumentStatus.Processing,
            "expired" => DocumentStatus.Expired,
            "failed" => DocumentStatus.Failed,
            _ => throw new JsonException($"Unknown document status: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, DocumentStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            DocumentStatus.Completed => "completed",
            DocumentStatus.Processing => "processing",
            DocumentStatus.Expired => "expired",
            DocumentStatus.Failed => "failed",
            _ => throw new JsonException(
                $"Unknown document status value: '{value}'.")
        });
    }
}

internal sealed class NullableDocumentStatusJsonConverter
    : NullableStructJsonConverter<DocumentStatus>
{
    protected override JsonConverter<DocumentStatus> InnerConverter
    {
        get;
    } = new DocumentStatusJsonConverter();
}
