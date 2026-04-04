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
        var parser = new PdfGateResponseParser(TestJsonOptions.CamelCase);
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
        var parser = new PdfGateResponseParser(TestJsonOptions.CamelCase);
        Assert.Throws<PdfGateException>(() =>
            parser.ParseObject(responseBody, ApiRoutes.ExtractPdfFormData));
    }

    [Fact]
    public void ParseObject_WhenPayloadIsValidJsonObject_ReturnsJsonElement()
    {
        const string responseBody = "{\"first_name\":\"John\"}";
        var parser = new PdfGateResponseParser(TestJsonOptions.CamelCase);

        JsonElement response =
            parser.ParseObject(responseBody, ApiRoutes.ExtractPdfFormData);

        Assert.Equal(JsonValueKind.Object, response.ValueKind);
        Assert.Equal("John", response.GetProperty("first_name").GetString());
    }

    [Fact]
    public void ParseEnvelope_WhenPayloadIsValid_ReturnsEnvelope()
    {
        const string responseBody =
            "{\"id\":\"env_123\",\"status\":\"created\",\"documents\":[],\"createdAt\":\"2024-02-13T15:56:12.607Z\"}";
        var parser = new PdfGateResponseParser(TestJsonOptions.CamelCase);

        PdfGate.net.Models.PdfGateEnvelope response =
            parser.ParseEnvelope(responseBody, ApiRoutes.CreateEnvelope);

        Assert.Equal(PdfGate.net.Models.EnvelopeStatus.Created,
            response.Status);
    }
}
