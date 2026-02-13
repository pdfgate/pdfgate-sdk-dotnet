using System.Text.Json;
using System.Text.Json.Serialization;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class WatermarkPdfFontTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    [Fact]
    public void WatermarkPdfRequest_WhenSerialized_UsesApiFontValue()
    {
        var request = new WatermarkPdfRequest
        {
            DocumentId = "doc_123",
            Type = WatermarkPdfType.Text,
            Text = "CONFIDENTIAL",
            Font = WatermarkPdfFont.TimesRoman
        };

        string json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"font\":\"times-roman\"", json,
            StringComparison.Ordinal);
    }

    [Fact]
    public void WatermarkPdfRequest_WhenDeserialized_ReadsApiFontValue()
    {
        const string json =
            "{\"documentId\":\"doc_123\",\"type\":\"text\",\"text\":\"CONFIDENTIAL\",\"font\":\"helvetica-bold\"}";

        WatermarkPdfRequest? request =
            JsonSerializer.Deserialize<WatermarkPdfRequest>(json,
                JsonOptions);

        Assert.NotNull(request);
        Assert.Equal(WatermarkPdfFont.HelveticaBold, request!.Font);
    }
}
