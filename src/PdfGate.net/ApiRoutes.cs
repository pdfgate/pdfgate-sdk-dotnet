using System.Globalization;

namespace PdfGate.net;

internal static class ApiRoutes
{
    internal const string GeneratePdf = "v1/generate/pdf";
    internal const string FlattenPdf = "forms/flatten";
    internal const string ExtractPdfFormData = "forms/extract-data";
    internal const string WatermarkPdf = "watermark/pdf";
    internal const string ProtectPdf = "protect/pdf";
    internal const string CompressPdf = "compress/pdf";
    internal const string UploadFile = "upload";

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
}
