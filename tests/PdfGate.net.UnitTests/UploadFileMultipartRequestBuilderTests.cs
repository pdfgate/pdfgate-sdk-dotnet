using System.Text;
using System.Text.Json;

using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class UploadFileMultipartRequestBuilderTests
{
    [Fact]
    public async Task Build_WhenRequestUsesUrl_AddsUrlMetadataAndPreSignedUrlParts()
    {
        var request = new UploadFileRequest
        {
            Url = new Uri("https://example.com/input.pdf"),
            Metadata = new { Source = "integration" },
            PreSignedUrlExpiresIn = 120
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var builder = new UploadFileMultipartRequestBuilder(request,
            jsonOptions);

        using MultipartFormDataContent form = builder.Build();

        HttpContent urlPart = GetPart(form, "url");
        string urlValue = await urlPart.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal("https://example.com/input.pdf", urlValue);

        HttpContent metadataPart = GetPart(form, "metadata");
        string metadataValue = await metadataPart.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal("{\"source\":\"integration\"}", metadataValue);

        HttpContent expiresInPart = GetPart(form, "preSignedUrlExpiresIn");
        string expiresInValue = await expiresInPart.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal("120", expiresInValue);

        Assert.DoesNotContain(form, part =>
            GetPartName(part) == "file");
    }

    [Fact]
    public async Task Build_WhenRequestUsesFile_AddsPdfFilePart()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("fake-pdf");
        var request = new UploadFileRequest
        {
            Content = new MemoryStream(bytes)
        };

        var builder = new UploadFileMultipartRequestBuilder(request,
            new JsonSerializerOptions());

        using MultipartFormDataContent form = builder.Build();

        HttpContent filePart = GetPart(form, "file");
        Assert.Equal("input.pdf",
            TrimQuotes(filePart.Headers.ContentDisposition?.FileName));
        Assert.Equal("application/pdf",
            filePart.Headers.ContentType?.MediaType);

        byte[] partBytes = await filePart.ReadAsByteArrayAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal(bytes, partBytes);

        Assert.DoesNotContain(form, part =>
            GetPartName(part) == "url");
    }

    private static HttpContent GetPart(
        MultipartFormDataContent form,
        string partName)
    {
        return Assert.Single(form, part => GetPartName(part) == partName);
    }

    private static string GetPartName(HttpContent content)
    {
        return TrimQuotes(content.Headers.ContentDisposition?.Name);
    }

    private static string TrimQuotes(string? value)
    {
        return value?.Trim('"') ?? string.Empty;
    }
}
