using System.Globalization;
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
    private readonly MultipartFormDataWriter _writer = new(jsonOptions);

    /// <summary>
    ///     Builds a multipart form payload for watermark operations.
    /// </summary>
    /// <returns>Multipart form content matching the watermark API contract.</returns>
    public MultipartFormDataContent Build()
    {
        var form = new MultipartFormDataContent();

        _writer.AddString(form, request.DocumentId, "documentId");
        _writer.AddString(form, request.Type.ToString().ToLowerInvariant(),
            "type");
        _writer.AddString(form, "true", "jsonResponse");

        _writer.AddFileIfNotNull(form, request.Watermark, "watermark");
        _writer.AddFileIfNotNull(form, request.FontFile, "fontFile");
        _writer.AddStringIfNotNull(form, request.Text, "text");

        if (request.Font is not null)
            _writer.AddStringIfNotNull(form, request.Font.Value.ToApiValue(),
                "font");
        if (request.FontSize.HasValue)
            _writer.AddStringIfNotNull(form, request.FontSize.Value.ToString(),
                "fontSize");
        _writer.AddStringIfNotNull(form, request.FontColor, "fontColor");
        if (request.Opacity.HasValue)
            _writer.AddStringIfNotNull(form, request.Opacity.Value.ToString(
                CultureInfo.InvariantCulture), "opacity");
        if (request.XPosition.HasValue)
            _writer.AddStringIfNotNull(form, request.XPosition.Value.ToString(),
                "xPosition");
        if (request.YPosition.HasValue)
            _writer.AddStringIfNotNull(form, request.YPosition.Value.ToString(),
                "yPosition");
        if (request.ImageWidth.HasValue)
            _writer.AddStringIfNotNull(form, request.ImageWidth.Value.ToString(),
                "imageWidth");
        if (request.ImageHeight.HasValue)
            _writer.AddStringIfNotNull(form,
                request.ImageHeight.Value.ToString(), "imageHeight");
        if (request.Rotate.HasValue)
            _writer.AddStringIfNotNull(form, request.Rotate.Value.ToString(
                CultureInfo.InvariantCulture), "rotate");
        if (request.PreSignedUrlExpiresIn.HasValue)
            _writer.AddStringIfNotNull(form,
                request.PreSignedUrlExpiresIn.Value.ToString(),
                "preSignedUrlExpiresIn");
        _writer.AddJsonIfNotNull(form, request.Metadata, "metadata");

        return form;
    }
}
