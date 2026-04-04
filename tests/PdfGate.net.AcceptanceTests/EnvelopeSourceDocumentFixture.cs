using PdfGate.net.Models;

using Xunit;

namespace PdfGate.net.AcceptanceTests;

/// <summary>
///     Shared fixture that creates a fillable source document suitable for envelope acceptance tests.
/// </summary>
public sealed class EnvelopeSourceDocumentFixture
{
    private PdfGateDocumentResponse? _document;

    /// <summary>
    ///     Returns an envelope-ready source document or skips the test when no client is available.
    /// </summary>
    public async Task<PdfGateDocumentResponse> GetDocumentOrSkipAsync(
        PdfGateClient? client)
    {
        if (_document != null)
            return _document;

        if (client is null)
            Assert.Skip("No client to get an example envelope source document.");

        var request = new GeneratePdfRequest
        {
            Html =
                """
                <html>
                <body style="font-family: Arial, sans-serif; padding: 40px;">
                  <h2>Agreement</h2>
                  <p>Please review and complete the required fields below.</p>
                  <div style="margin-top: 30px;">
                    <label>Full Name</label><br />
                    <input type="text" name="recipient-name" style="width: 300px; height: 30px;" />
                  </div>
                  <div style="margin-top: 30px;">
                    <label>Signature</label><br />
                    <pdfgate-signature-field name="signature" style="width: 200px; height: 200px;"></pdfgate-signature-field>
                  </div>
                  <div style="margin-top: 30px;">
                    <label>Date</label><br />
                    <input type="datetime-local" name="signature-date" pdfgate-auto-fill="true" style="width: 200px; height: 30px;" />
                  </div>
                </body>
                </html>
                """,
            EnableFormFields = true
        };

        _document = await client.GeneratePdfAsync(request,
            TestContext.Current.CancellationToken);

        return _document;
    }
}
