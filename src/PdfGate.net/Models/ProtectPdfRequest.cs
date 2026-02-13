using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the protect PDF endpoint.
/// </summary>
public sealed record ProtectPdfRequest
{
    /// <summary>
    ///     Existing stored PDF document ID.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Encryption algorithm.
    /// </summary>
    [JsonConverter(typeof(ProtectPdfEncryptionAlgorithmJsonConverter))]
    public ProtectPdfEncryptionAlgorithm? Algorithm
    {
        get;
        init;
    }

    /// <summary>
    ///     Password required to open the protected PDF.
    /// </summary>
    public string? UserPassword
    {
        get;
        init;
    }

    /// <summary>
    ///     Owner password with full control over document permissions.
    /// </summary>
    public string? OwnerPassword
    {
        get;
        init;
    }

    /// <summary>
    ///     Disables printing when true.
    /// </summary>
    public bool? DisablePrint
    {
        get;
        init;
    }

    /// <summary>
    ///     Disables copying when true.
    /// </summary>
    public bool? DisableCopy
    {
        get;
        init;
    }

    /// <summary>
    ///     Disables editing when true.
    /// </summary>
    public bool? DisableEditing
    {
        get;
        init;
    }

    /// <summary>
    ///     Encrypts metadata when true.
    /// </summary>
    public bool? EncryptMetadata
    {
        get;
        init;
    }

    /// <summary>
    ///     Always requests JSON metadata responses from the API.
    /// </summary>
    public bool JsonResponse
    {
        get;
    } = true;

    /// <summary>
    ///     Pre-signed URL expiration in seconds.
    /// </summary>
    public long? PreSignedUrlExpiresIn
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the protected document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}

/// <summary>
///     Supported PDF encryption algorithms.
/// </summary>
public enum ProtectPdfEncryptionAlgorithm
{
    /// <summary>
    ///     AES-256 encryption.
    /// </summary>
    Aes256,

    /// <summary>
    ///     AES-128 encryption.
    /// </summary>
    Aes128
}

internal sealed class
    ProtectPdfEncryptionAlgorithmJsonConverter
    : JsonConverter<ProtectPdfEncryptionAlgorithm>
{
    public override ProtectPdfEncryptionAlgorithm Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "AES256" => ProtectPdfEncryptionAlgorithm.Aes256,
            "AES128" => ProtectPdfEncryptionAlgorithm.Aes128,
            _ => throw new JsonException(
                $"Unknown protect PDF encryption algorithm: '{value}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer,
        ProtectPdfEncryptionAlgorithm value,
        JsonSerializerOptions options)
    {
        var apiValue = value switch
        {
            ProtectPdfEncryptionAlgorithm.Aes256 => "AES256",
            ProtectPdfEncryptionAlgorithm.Aes128 => "AES128",
            _ => throw new JsonException(
                $"Unknown protect PDF encryption algorithm value: '{value}'.")
        };

        writer.WriteStringValue(apiValue);
    }
}
