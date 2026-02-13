using System.Text.Json;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class PdfGateResponseParserTests
{
    [Theory]
    [InlineData("{")]
    [InlineData("{}")]
    public void
        Parse_WhenPayloadIsMalformedOrInvalid_ThrowsPdfGateException(
            string responseBody)
    {
        var parser = new PdfGateResponseParser(new JsonSerializerOptions());
        Assert.Throws<PdfGateException>(() =>
            parser.Parse(responseBody, "example/path"));
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    public void
        ParseObject_WhenPayloadIsMalformedOrNotObject_ThrowsPdfGateException(
            string responseBody)
    {
        var parser = new PdfGateResponseParser(new JsonSerializerOptions());
        Assert.Throws<PdfGateException>(() =>
            parser.ParseObject(responseBody, "forms/extract-data"));
    }

    [Fact]
    public void ParseObject_WhenPayloadIsValidJsonObject_ReturnsJsonElement()
    {
        const string responseBody = "{\"first_name\":\"John\"}";
        var parser = new PdfGateResponseParser(new JsonSerializerOptions());

        JsonElement response =
            parser.ParseObject(responseBody, "forms/extract-data");

        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.Equal("John", response.GetProperty("first_name").GetString());
    }
}
