using System.Security.Cryptography;
using System.Text;

namespace PdfGate.net;

/// <summary>
///     Helpers for working with PDFGate webhooks.
/// </summary>
public static class PdfGateWebhook
{
    /// <summary>
    ///     The header name used to send webhook signatures.
    /// </summary>
    public const string SignatureHeaderName = "x-pdfgate-signature";

    private static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Verifies a webhook signature against the raw request body.
    /// </summary>
    /// <param name="secret">Webhook signing secret.</param>
    /// <param name="signatureHeader">Value of the <c>x-pdfgate-signature</c> header.</param>
    /// <param name="payload">Raw request body exactly as received.</param>
    public static void VerifySignature(string secret, string signatureHeader,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("A webhook secret is required.",
                nameof(secret));

        if (payload is null)
            throw new ArgumentNullException(nameof(payload));

        VerifySignature(secret, signatureHeader, Encoding.UTF8.GetBytes(payload),
            DateTimeOffset.UtcNow, DefaultTolerance);
    }

    /// <summary>
    ///     Verifies a webhook signature against the raw request body bytes.
    /// </summary>
    /// <param name="secret">Webhook signing secret.</param>
    /// <param name="signatureHeader">Value of the <c>x-pdfgate-signature</c> header.</param>
    /// <param name="payload">Raw request body exactly as received.</param>
    public static void VerifySignature(string secret, string signatureHeader,
        byte[] payload)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("A webhook secret is required.",
                nameof(secret));

        Guard.ThrowIfNull(payload);

        VerifySignature(secret, signatureHeader, payload, DateTimeOffset.UtcNow,
            DefaultTolerance);
    }

    internal static void VerifySignature(string secret, string signatureHeader,
        byte[] payload, DateTimeOffset now, TimeSpan tolerance)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            throw new PdfGateException("Missing signature.");

        if (tolerance < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(tolerance),
                "Tolerance must not be negative.");

        long? timestamp = null;
        var signatures = new List<string>();

        foreach (string part in signatureHeader.Split(','))
        {
            string[] pair = part.Split(['='], 2, StringSplitOptions.None);
            if (pair.Length != 2)
                continue;

            string key = pair[0].Trim();
            string value = pair[1].Trim();

            if (key == "t" && long.TryParse(value, out long parsedTimestamp))
                timestamp = parsedTimestamp;

            if (key == "v1" && !string.IsNullOrWhiteSpace(value))
                signatures.Add(value);
        }

        if (!timestamp.HasValue)
            throw new PdfGateException("Missing timestamp.");

        if (signatures.Count == 0)
            throw new PdfGateException("Missing signature.");

        long ageInSeconds = now.ToUnixTimeSeconds() - timestamp.Value;
        if (ageInSeconds > (long)tolerance.TotalSeconds)
            throw new PdfGateException("Signature expired.");

        byte[] expectedSignature = ComputeSignature(secret, timestamp.Value,
            payload);

        foreach (string signature in signatures)
        {
            if (TryDecodeHex(signature, out byte[]? providedSignature)
                && providedSignature is not null
                && FixedTimeEquals(expectedSignature, providedSignature))
                return;
        }

        throw new PdfGateException("Invalid signature.");
    }

    private static byte[] ComputeSignature(string secret, long timestamp,
        byte[] payload)
    {
        byte[] prefix = Encoding.UTF8.GetBytes($"{timestamp}.");
        byte[] signedPayload = new byte[prefix.Length + payload.Length];
        Buffer.BlockCopy(prefix, 0, signedPayload, 0, prefix.Length);
        Buffer.BlockCopy(payload, 0, signedPayload, prefix.Length,
            payload.Length);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return hmac.ComputeHash(signedPayload);
    }

    private static bool TryDecodeHex(string value, out byte[]? bytes)
    {
        bytes = null;
        if (value.Length % 2 != 0)
            return false;

        var result = new byte[value.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            if (!TryGetHexValue(value[i * 2], out int high)
                || !TryGetHexValue(value[i * 2 + 1], out int low))
                return false;

            result[i] = (byte)((high << 4) | low);
        }

        bytes = result;
        return true;
    }

    private static bool TryGetHexValue(char c, out int value)
    {
        if (c is >= '0' and <= '9')
        {
            value = c - '0';
            return true;
        }

        if (c is >= 'a' and <= 'f')
        {
            value = c - 'a' + 10;
            return true;
        }

        if (c is >= 'A' and <= 'F')
        {
            value = c - 'A' + 10;
            return true;
        }

        value = 0;
        return false;
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
            return false;

        int diff = 0;
        for (int i = 0; i < left.Length; i++)
            diff |= left[i] ^ right[i];

        return diff == 0;
    }
}
