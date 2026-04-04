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
        return Parse<PdfGateDocumentResponse>(content, url, document =>
            document.Status is not null);
    }

    /// <summary>
    ///     Parse JSON responses into envelope metadata.
    /// </summary>
    public PdfGateEnvelope ParseEnvelope(string content, string url)
    {
        return Parse<PdfGateEnvelope>(content, url, envelope =>
            envelope.Status is not null);
    }

    /// <summary>
    ///     Parse JSON responses into typed models.
    /// </summary>
    public T Parse<T>(string content, string url, Func<T, bool>? isValid = null)
    {
        try
        {
            var response = JsonSerializer.Deserialize<T>(content, jsonOptions);
            if (response is null || isValid is not null && !isValid(response))
                throw new PdfGateException(
                    $"The API returned an invalid response for endpoint '{url}'.");

            return response;
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
