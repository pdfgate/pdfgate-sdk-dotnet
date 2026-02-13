using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     PdfGateResponseParser parses JSON responses into typed models
/// </summary>
internal sealed class PdfGateResponseParser(JsonSerializerOptions jsonOptions)
{
    /// <summary>
    ///     Parse JSON responses into the metadata of a PDFGate document
    /// </summary>
    public PdfGateDocumentResponse Parse(string content, string url)
    {
        try
        {
            var document =
                JsonSerializer.Deserialize<PdfGateDocumentResponse>(content,
                    jsonOptions);
            if (document is null || document.Status is null)
                throw new PdfGateException(
                    $"The API returned an invalid response for endpoint '{url}'.");

            return document;
        }
        catch (PdfGateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfGateException(
                $"Failed to parse response from endpoint '{url}'.", ex);
        }
    }

    /// <summary>
    ///     Parse JSON responses into a generic JSON object payload.
    /// </summary>
    public JsonElement ParseObject(string content, string url)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new PdfGateException(
                    $"The API returned an invalid response for endpoint '{url}'.");

            return document.RootElement.Clone();
        }
        catch (PdfGateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PdfGateException(
                $"Failed to parse response from endpoint '{url}'.", ex);
        }
    }
}
