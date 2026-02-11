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
}
