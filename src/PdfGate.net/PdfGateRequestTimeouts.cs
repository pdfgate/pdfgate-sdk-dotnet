namespace PdfGate.net;

/// <summary>
///     Per-endpoint request timeouts used by <see cref="PdfGateClient" /> when wrapping user cancellation tokens.
/// </summary>
public sealed class PdfGateRequestTimeouts
{
    /// <summary>
    ///     Timeout for generate PDF requests.
    /// </summary>
    public TimeSpan GeneratePdf
    {
        get;
        init;
    } = TimeSpan.FromMinutes(15);

    /// <summary>
    ///     Timeout for flatten PDF requests.
    /// </summary>
    public TimeSpan FlattenPdf
    {
        get;
        init;
    } = TimeSpan.FromMinutes(3);

    /// <summary>
    ///     Timeout for watermark PDF requests.
    /// </summary>
    public TimeSpan WatermarkPdf
    {
        get;
        init;
    } = TimeSpan.FromMinutes(3);

    /// <summary>
    ///     Timeout for protect PDF requests.
    /// </summary>
    public TimeSpan ProtectPdf
    {
        get;
        init;
    } = TimeSpan.FromMinutes(3);

    /// <summary>
    ///     Timeout for compress PDF requests.
    /// </summary>
    public TimeSpan CompressPdf
    {
        get;
        init;
    } = TimeSpan.FromMinutes(3);

    /// <summary>
    ///     Timeout for all remaining endpoints.
    /// </summary>
    public TimeSpan DefaultEndpoint
    {
        get;
        init;
    } = TimeSpan.FromSeconds(60);

    internal void Validate()
    {
        ValidatePositiveTimeout(GeneratePdf, nameof(GeneratePdf));
        ValidatePositiveTimeout(FlattenPdf, nameof(FlattenPdf));
        ValidatePositiveTimeout(WatermarkPdf, nameof(WatermarkPdf));
        ValidatePositiveTimeout(ProtectPdf, nameof(ProtectPdf));
        ValidatePositiveTimeout(CompressPdf, nameof(CompressPdf));
        ValidatePositiveTimeout(DefaultEndpoint, nameof(DefaultEndpoint));
    }

    private static void ValidatePositiveTimeout(TimeSpan timeout,
        string propertyName)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(propertyName,
                "Timeout must be greater than zero.");
    }
}
