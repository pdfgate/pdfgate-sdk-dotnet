using System.Security.Cryptography;
using System.Text;

using Xunit;

namespace PdfGate.net.UnitTests;

public sealed class PdfGateWebhookTests
{
    private const string Secret = "whsecret_test";
    private const string Payload = "{\"id\":\"evt_123\"}";

    [Fact]
    public void VerifySignature_WhenSignatureIsValid_Succeeds()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string header = BuildHeader(Secret, timestamp, Payload);

        PdfGateWebhook.VerifySignature(Secret, header, Payload);
    }

    [Fact]
    public void
        VerifySignature_WhenMultipleV1SignaturesAndOneIsValid_Succeeds()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string validSignature = ComputeSignature(Secret, timestamp, Payload);
        string header =
            $"t={timestamp},v1=deadbeef,v1={validSignature},v1=badc0ffee0";

        PdfGateWebhook.VerifySignature(Secret, header, Payload);
    }

    [Fact]
    public void VerifySignature_WhenHeaderIsMissingValidSignature_Fails()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string header = $"t={timestamp},v0=abcdef";

        PdfGateException exception = Assert.Throws<PdfGateException>(() =>
            PdfGateWebhook.VerifySignature(Secret, header, Payload));

        Assert.Equal("Missing signature.", exception.Message);
    }

    [Fact]
    public void VerifySignature_WhenHeaderIsMissingTimestamp_Fails()
    {
        string header = $"v1={ComputeSignature(Secret, 1712345678, Payload)}";

        PdfGateException exception = Assert.Throws<PdfGateException>(() =>
            PdfGateWebhook.VerifySignature(Secret, header, Payload));

        Assert.Equal("Missing timestamp.", exception.Message);
    }

    [Fact]
    public void VerifySignature_WhenSignatureIsExpired_Fails()
    {
        long timestamp = DateTimeOffset.UtcNow.AddMinutes(-6)
            .ToUnixTimeSeconds();
        string header = BuildHeader(Secret, timestamp, Payload);

        PdfGateException exception = Assert.Throws<PdfGateException>(() =>
            PdfGateWebhook.VerifySignature(Secret, header, Payload));

        Assert.Equal("Signature expired.", exception.Message);
    }

    [Fact]
    public void VerifySignature_WhenSignatureIsInvalid_Fails()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string header = $"t={timestamp},v1=deadbeef";

        PdfGateException exception = Assert.Throws<PdfGateException>(() =>
            PdfGateWebhook.VerifySignature(Secret, header, Payload));

        Assert.Equal("Invalid signature.", exception.Message);
    }

    private static string BuildHeader(string secret, long timestamp,
        string payload)
    {
        return $"t={timestamp},v1={ComputeSignature(secret, timestamp, payload)}";
    }

    private static string ComputeSignature(string secret, long timestamp,
        string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] signature = hmac.ComputeHash(
            Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        var builder = new StringBuilder(signature.Length * 2);
        foreach (byte b in signature)
            builder.AppendFormat("{0:x2}", b);

        return builder.ToString();
    }
}
