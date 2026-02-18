namespace PdfGate.net;

/// <summary>
/// Provides framework-compatible argument guard helpers.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> when <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Optional parameter name. Inferred from caller when omitted.</param>
    public static void ThrowIfNull<T>(T value, string? paramName = null)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }
}
