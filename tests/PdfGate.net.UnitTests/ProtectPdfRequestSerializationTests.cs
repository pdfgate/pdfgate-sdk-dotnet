using System.Text.Json;
using System.Text.Json.Serialization;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class ProtectPdfRequestSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    [Fact]
    public void Serialize_WhenAlgorithmIsAes256_WritesAllCapsApiValue()
    {
        var request = new ProtectPdfRequest
        {
            DocumentId = "doc-id",
            Algorithm = ProtectPdfEncryptionAlgorithm.Aes256
        };

        string json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"algorithm\":\"AES256\"", json,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_WhenAlgorithmIsAes128_WritesAllCapsApiValue()
    {
        var request = new ProtectPdfRequest
        {
            DocumentId = "doc-id",
            Algorithm = ProtectPdfEncryptionAlgorithm.Aes128
        };

        string json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"algorithm\":\"AES128\"", json,
            StringComparison.Ordinal);
    }
}
