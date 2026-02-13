using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class PdfGateFileTests
{
    [Fact]
    public void Type_WhenProvided_ReturnsProvidedValue()
    {
        var file = new PdfGateFile
        {
            Name = "watermark.png",
            Content = Stream.Null,
            Type = "custom/type"
        };

        Assert.Equal("custom/type", file.Type);
    }

    [Fact]
    public void Type_WhenMissing_InfersMimeTypeFromFileName()
    {
        var file = new PdfGateFile
        {
            Name = "watermark.png",
            Content = Stream.Null
        };

        Assert.Equal("image/png", file.Type);
    }

    [Fact]
    public void Type_WhenMissing_InfersMimeTypeCaseInsensitive()
    {
        var file = new PdfGateFile
        {
            Name = "WATERMARK.JPEG",
            Content = Stream.Null
        };

        Assert.Equal("image/jpeg", file.Type);
    }

    [Fact]
    public void Type_WhenExtensionIsUnknown_DefaultsToOctetStream()
    {
        var file = new PdfGateFile
        {
            Name = "archive.unknown",
            Content = Stream.Null
        };

        Assert.Equal("application/octet-stream", file.Type);
    }
}
