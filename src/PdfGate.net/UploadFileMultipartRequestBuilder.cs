using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Builds multipart request content for the upload endpoint.
/// </summary>
internal sealed class UploadFileMultipartRequestBuilder(
    UploadFileRequest request,
    JsonSerializerOptions jsonOptions)
{
    private readonly MultipartFormDataWriter _writer = new(jsonOptions);

    /// <summary>
    ///     Builds a multipart form payload for file upload operations.
    /// </summary>
    /// <returns>Multipart form content matching the upload API contract.</returns>
    public MultipartFormDataContent Build()
    {
        var form = new MultipartFormDataContent();

        if (request.Url is not null)
            _writer.AddString(form, request.Url.AbsoluteUri, "url");
        else
            _writer.AddStreamFile(form, request.Content, "file", "input.pdf",
                "application/pdf");

        _writer.AddJsonIfNotNull(form, request.Metadata, "metadata");

        _writer.AddStringIfNotNull(form,
            request.PreSignedUrlExpiresIn?.ToString(),
            "preSignedUrlExpiresIn");

        return form;
    }
}
