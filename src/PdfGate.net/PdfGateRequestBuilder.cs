using System.Text;
using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Builds HTTP request payloads for PDFGate endpoints.
/// </summary>
internal static class PdfGateRequestBuilder
{
    /// <summary>
    ///     Builds multipart form data for the flatten PDF endpoint.
    /// </summary>
    /// <param name="request">Flatten PDF request payload.</param>
    /// <param name="jsonOptions">Serializer options used for metadata serialization.</param>
    /// <returns>Multipart form data content for the flatten request.</returns>
    public static MultipartFormDataContent BuildFlattenPdfFormData(
        FlattenPdfRequest request,
        JsonSerializerOptions jsonOptions)
    {
        var form = new MultipartFormDataContent();

        form.Add(
            new StringContent(
                request.JsonResponse.ToString().ToLowerInvariant(),
                Encoding.UTF8),
            "jsonResponse");

        if (request.PreSignedUrlExpiresIn.HasValue)
            form.Add(
                new StringContent(
                    request.PreSignedUrlExpiresIn.Value.ToString(),
                    Encoding.UTF8),
                "preSignedUrlExpiresIn");

        if (request.Metadata is not null)
            form.Add(
                new StringContent(
                    JsonSerializer.Serialize(request.Metadata, jsonOptions),
                    Encoding.UTF8),
                "metadata");

        if (!string.IsNullOrWhiteSpace(request.DocumentId))
            form.Add(
                new StringContent(request.DocumentId, Encoding.UTF8),
                "documentId");

        return form;
    }
}
