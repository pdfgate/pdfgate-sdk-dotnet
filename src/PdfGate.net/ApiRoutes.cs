using System.Globalization;

namespace PdfGate.net;

internal static class ApiRoutes
{
    internal const string GeneratePdf = "v1/generate/pdf";
    internal const string CreateEnvelope = "envelope";
    internal const string FlattenPdf = "forms/flatten";
    internal const string AddFormFields = "forms/fields";
    internal const string ExtractPdfFormData = "forms/extract-data";
    internal const string WatermarkPdf = "watermark/pdf";
    internal const string ProtectPdf = "protect/pdf";
    internal const string CompressPdf = "compress/pdf";
    internal const string UploadFile = "upload";
    internal const string Webhook = "webhook";

    internal static string GetDocument(string documentId,
        long? preSignedUrlExpiresIn = null)
    {
        var escapedDocumentId = Uri.EscapeDataString(documentId);
        if (!preSignedUrlExpiresIn.HasValue)
            return $"document/{escapedDocumentId}";

        return
            $"document/{escapedDocumentId}?preSignedUrlExpiresIn={preSignedUrlExpiresIn.Value.ToString(CultureInfo.InvariantCulture)}";
    }

    internal static string GetFile(string documentId)
    {
        return $"file/{Uri.EscapeDataString(documentId)}";
    }

    internal static string SendEnvelope(string envelopeId)
    {
        return $"envelope/{Uri.EscapeDataString(envelopeId)}/send";
    }

    internal static string GetEnvelope(string envelopeId)
    {
        return $"envelope/{Uri.EscapeDataString(envelopeId)}";
    }

    internal static string VoidEnvelope(string envelopeId)
    {
        return $"envelope/{Uri.EscapeDataString(envelopeId)}/void";
    }

    internal static string DeleteEnvelope(string envelopeId)
    {
        return $"envelope/{Uri.EscapeDataString(envelopeId)}";
    }

    internal static string DeleteDocument(string documentId)
    {
        return $"document/{Uri.EscapeDataString(documentId)}";
    }

    internal static string GetWebhook(string webhookId)
    {
        return $"webhook/{Uri.EscapeDataString(webhookId)}";
    }
}
