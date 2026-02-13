using System.Text.Json;

namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the watermark PDF endpoint.
/// </summary>
public sealed record WatermarkPdfRequest
{
    /// <summary>
    ///     Existing stored PDF document ID.
    /// </summary>
    public required string DocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark type.
    /// </summary>
    public required WatermarkPdfType Type
    {
        get;
        init;
    }

    /// <summary>
    ///     Image watermark file payload used when <see cref="Type" /> is <see cref="WatermarkPdfType.Image" />.
    /// </summary>
    public PdfGateFile? Watermark
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom font file payload used for text watermarks.
    /// </summary>
    public PdfGateFile? FontFile
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark text used when <see cref="Type" /> is <see cref="WatermarkPdfType.Text" />.
    /// </summary>
    public string? Text
    {
        get;
        init;
    }

    /// <summary>
    ///     Text watermark font.
    /// </summary>
    public WatermarkPdfFont? Font
    {
        get;
        init;
    }

    /// <summary>
    ///     Text watermark font size.
    /// </summary>
    public int? FontSize
    {
        get;
        init;
    }

    /// <summary>
    ///     Text watermark font color.
    /// </summary>
    public string? FontColor
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark opacity.
    /// </summary>
    public double? Opacity
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark horizontal position.
    /// </summary>
    public int? XPosition
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark vertical position.
    /// </summary>
    public int? YPosition
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark image width.
    /// </summary>
    public int? ImageWidth
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark image height.
    /// </summary>
    public int? ImageHeight
    {
        get;
        init;
    }

    /// <summary>
    ///     Watermark rotation in degrees.
    /// </summary>
    public double? Rotate
    {
        get;
        init;
    }

    /// <summary>
    ///     Always requests JSON metadata responses from the API.
    /// </summary>
    public bool JsonResponse
    {
        get;
    } = true;

    /// <summary>
    ///     Pre-signed URL expiration in seconds.
    /// </summary>
    public long? PreSignedUrlExpiresIn
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the watermarked document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}

/// <summary>
///     Supported watermark types.
/// </summary>
public enum WatermarkPdfType
{
    /// <summary>
    ///     Text watermark.
    /// </summary>
    Text,

    /// <summary>
    ///     Image watermark.
    /// </summary>
    Image
}

/// <summary>
///     Standard built-in fonts supported by PDF watermarking.
/// </summary>
public enum WatermarkPdfFont
{
    /// <summary>
    ///     Times Roman.
    /// </summary>
    TimesRoman,

    /// <summary>
    ///     Times Bold.
    /// </summary>
    TimesBold,

    /// <summary>
    ///     Times Italic.
    /// </summary>
    TimesItalic,

    /// <summary>
    ///     Times Bold Italic.
    /// </summary>
    TimesBoldItalic,

    /// <summary>
    ///     Helvetica.
    /// </summary>
    Helvetica,

    /// <summary>
    ///     Helvetica Bold.
    /// </summary>
    HelveticaBold,

    /// <summary>
    ///     Helvetica Oblique.
    /// </summary>
    HelveticaOblique,

    /// <summary>
    ///     Helvetica Bold Oblique.
    /// </summary>
    HelveticaBoldOblique,

    /// <summary>
    ///     Courier.
    /// </summary>
    Courier,

    /// <summary>
    ///     Courier Bold.
    /// </summary>
    CourierBold,

    /// <summary>
    ///     Courier Oblique.
    /// </summary>
    CourierOblique,

    /// <summary>
    ///     Courier Bold Oblique.
    /// </summary>
    CourierBoldOblique
}

internal static class WatermarkPdfFontExtensions
{
    public static string ToApiValue(this WatermarkPdfFont font)
    {
        return font switch
        {
            WatermarkPdfFont.TimesRoman => "times-roman",
            WatermarkPdfFont.TimesBold => "times-bold",
            WatermarkPdfFont.TimesItalic => "times-italic",
            WatermarkPdfFont.TimesBoldItalic => "times-bolditalic",
            WatermarkPdfFont.Helvetica => "helvetica",
            WatermarkPdfFont.HelveticaBold => "helvetica-bold",
            WatermarkPdfFont.HelveticaOblique => "helvetica-oblique",
            WatermarkPdfFont.HelveticaBoldOblique => "helvetica-boldoblique",
            WatermarkPdfFont.Courier => "courier",
            WatermarkPdfFont.CourierBold => "courier-bold",
            WatermarkPdfFont.CourierOblique => "courier-oblique",
            WatermarkPdfFont.CourierBoldOblique => "courier-boldoblique",
            _ => throw new JsonException($"Unknown watermark font: '{font}'.")
        };
    }
}
