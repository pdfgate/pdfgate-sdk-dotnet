using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Builds multipart request content for the watermark endpoint.
/// </summary>
internal sealed class WatermarkPdfMultipartRequestBuilder(
    WatermarkPdfRequest request,
    JsonSerializerOptions jsonOptions)
{
    /// <summary>
    ///     Builds a multipart form payload for watermark operations.
    /// </summary>
    /// <returns>Multipart form content matching the watermark API contract.</returns>
    public MultipartFormDataContent Build()
    {
        var form = new MultipartFormDataContent
        {
            {
                new StringContent(request.DocumentId, Encoding.UTF8),
                "documentId"
            },
            {
                new StringContent(request.Type.ToString().ToLowerInvariant(),
                    Encoding.UTF8),
                "type"
            },
            { new StringContent("true", Encoding.UTF8), "jsonResponse" }
        };

        AddFilePart(form, request.Watermark, "watermark");
        AddFilePart(form, request.FontFile, "fontFile");
        AddStringPartIfNotNull(form, request.Text, "text");

        if (request.Font is not null)
            AddStringPartIfNotNull(form, request.Font.Value.ToApiValue(),
                "font");
        if (request.FontSize.HasValue)
            AddStringPartIfNotNull(form, request.FontSize.Value.ToString(),
                "fontSize");
        AddStringPartIfNotNull(form, request.FontColor, "fontColor");
        if (request.Opacity.HasValue)
            AddStringPartIfNotNull(form, request.Opacity.Value.ToString(
                CultureInfo.InvariantCulture), "opacity");
        if (request.XPosition.HasValue)
            AddStringPartIfNotNull(form, request.XPosition.Value.ToString(),
                "xPosition");
        if (request.YPosition.HasValue)
            AddStringPartIfNotNull(form, request.YPosition.Value.ToString(),
                "yPosition");
        if (request.ImageWidth.HasValue)
            AddStringPartIfNotNull(form, request.ImageWidth.Value.ToString(),
                "imageWidth");
        if (request.ImageHeight.HasValue)
            AddStringPartIfNotNull(form,
                request.ImageHeight.Value.ToString(), "imageHeight");
        if (request.Rotate.HasValue)
            AddStringPartIfNotNull(form, request.Rotate.Value.ToString(
                CultureInfo.InvariantCulture), "rotate");
        if (request.PreSignedUrlExpiresIn.HasValue)
            AddStringPartIfNotNull(form,
                request.PreSignedUrlExpiresIn.Value.ToString(),
                "preSignedUrlExpiresIn");
        if (request.Metadata is not null)
            AddStringPartIfNotNull(form,
                JsonSerializer.Serialize(request.Metadata, jsonOptions),
                "metadata");

        return form;
    }

    private static void AddFilePart(
        MultipartFormDataContent form,
        PdfGateFile? file,
        string partName)
    {
        if (file is null)
            return;

        var fileContent = new StreamContent(file.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.Type);
        form.Add(fileContent, partName, file.Name);
    }

    private static void AddStringPartIfNotNull(
        MultipartFormDataContent form,
        string? value,
        string partName)
    {
        if (value is null)
            return;

        form.Add(new StringContent(value, Encoding.UTF8), partName);
    }
}
