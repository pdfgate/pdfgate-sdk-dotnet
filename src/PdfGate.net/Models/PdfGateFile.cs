namespace PdfGate.net.Models;

/// <summary>
///     File payload used by PDFGate multipart endpoints.
/// </summary>
public sealed record PdfGateFile
{
    private readonly string? _type;

    /// <summary>
    ///     File name sent to the API.
    /// </summary>
    public required string Name
    {
        get;
        init;
    }

    /// <summary>
    ///     File content stream.
    ///     The stream is owned by the caller.
    /// </summary>
    public required Stream Content
    {
        get;
        init;
    }

    /// <summary>
    ///     File MIME type. If omitted, inferred from <see cref="Name" />.
    /// </summary>
    public string Type
    {
        get => _type ?? InferMimeTypeFromFileName(Name);
        init => _type = value;
    }

    private static string InferMimeTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            return "application/octet-stream";

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".ttf" => "font/ttf",
            ".otf" => "font/otf",
            _ => "application/octet-stream"
        };
    }
}
