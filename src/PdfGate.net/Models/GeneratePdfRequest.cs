using System.Text.Json.Serialization;

namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the generate PDF endpoint.
/// </summary>
public sealed record GeneratePdfRequest
{
    /// <summary>
    ///     HTML input used to create a PDF.
    /// </summary>
    public string? Html
    {
        get;
        init;
    }

    /// <summary>
    ///     Public URL used to create a PDF.
    /// </summary>
    public string? Url
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
    ///     Page size type, for example <c>a4</c>.
    /// </summary>
    public GeneratePdfPageSizeType? PageSizeType
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom page width in pixels.
    /// </summary>
    public int? Width
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom page height in pixels.
    /// </summary>
    public int? Height
    {
        get;
        init;
    }

    /// <summary>
    ///     Page orientation (<c>portrait</c> or <c>landscape</c>).
    /// </summary>
    public GeneratePdfFileOrientation? Orientation
    {
        get;
        init;
    }

    /// <summary>
    ///     HTML header displayed on each page.
    /// </summary>
    public string? Header
    {
        get;
        init;
    }

    /// <summary>
    ///     HTML footer displayed on each page.
    /// </summary>
    public string? Footer
    {
        get;
        init;
    }

    /// <summary>
    ///     Page margin configuration.
    /// </summary>
    public GeneratePdfPageMargin? Margin
    {
        get;
        init;
    }

    /// <summary>
    ///     Render timeout in milliseconds.
    /// </summary>
    public int? Timeout
    {
        get;
        init;
    }

    /// <summary>
    ///     JavaScript to execute before rendering.
    /// </summary>
    public string? Javascript
    {
        get;
        init;
    }

    /// <summary>
    ///     Additional CSS to apply when rendering.
    /// </summary>
    public string? Css
    {
        get;
        init;
    }

    /// <summary>
    ///     CSS media type to emulate (<c>screen</c> or <c>print</c>).
    /// </summary>
    public GeneratePdfEmulateMediaType? EmulateMediaType
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom HTTP headers used when loading the target URL.
    /// </summary>
    public Dictionary<string, string>? HttpHeaders
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom metadata attached to the generated document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }

    /// <summary>
    ///     CSS selector to wait for before rendering.
    /// </summary>
    public string? WaitForSelector
    {
        get;
        init;
    }

    /// <summary>
    ///     CSS selector to click before rendering.
    /// </summary>
    public string? ClickSelector
    {
        get;
        init;
    }

    /// <summary>
    ///     Multi-step click selector configuration.
    /// </summary>
    public GeneratePdfClickSelectorChainSetup? ClickSelectorChainSetup
    {
        get;
        init;
    }

    /// <summary>
    ///     Waits for network idle before rendering.
    /// </summary>
    public bool? WaitForNetworkIdle
    {
        get;
        init;
    }

    /// <summary>
    ///     Enables interactive form fields in the generated PDF.
    /// </summary>
    public bool? EnableFormFields
    {
        get;
        init;
    }

    /// <summary>
    ///     Delay in milliseconds before rendering.
    /// </summary>
    public int? Delay
    {
        get;
        init;
    }

    /// <summary>
    ///     Waits for images to load before rendering.
    /// </summary>
    public bool? LoadImages
    {
        get;
        init;
    }

    /// <summary>
    ///     Rendering scale factor.
    /// </summary>
    public double? Scale
    {
        get;
        init;
    }

    /// <summary>
    ///     Page ranges to render, for example <c>1-3</c> or <c>1,3,5</c>.
    /// </summary>
    public string? PageRanges
    {
        get;
        init;
    }

    /// <summary>
    ///     Includes page background graphics.
    /// </summary>
    public bool? PrintBackground
    {
        get;
        init;
    }

    /// <summary>
    ///     Custom user-agent string.
    /// </summary>
    public string? UserAgent
    {
        get;
        init;
    }

    /// <summary>
    ///     Authentication credentials for protected web content.
    /// </summary>
    public GeneratePdfAuthentication? Authentication
    {
        get;
        init;
    }

    /// <summary>
    ///     Viewport dimensions used for rendering.
    /// </summary>
    public GeneratePdfViewport? Viewport
    {
        get;
        init;
    }
}

/// <summary>
///     Margin values applied to generated pages.
/// </summary>
public sealed record GeneratePdfPageMargin
{
    /// <summary>
    ///     Top margin value.
    /// </summary>
    public string? Top
    {
        get;
        init;
    }

    /// <summary>
    ///     Bottom margin value.
    /// </summary>
    public string? Bottom
    {
        get;
        init;
    }

    /// <summary>
    ///     Left margin value.
    /// </summary>
    public string? Left
    {
        get;
        init;
    }

    /// <summary>
    ///     Right margin value.
    /// </summary>
    public string? Right
    {
        get;
        init;
    }
}

/// <summary>
///     Click selector chain configuration.
/// </summary>
public sealed record GeneratePdfClickSelectorChainSetup
{
    /// <summary>
    ///     Ignores failing chains when true.
    /// </summary>
    public bool? IgnoreFailingChains
    {
        get;
        init;
    }

    /// <summary>
    ///     Ordered chains of selectors to click.
    /// </summary>
    public IReadOnlyList<GeneratePdfClickSelectorChain>? Chains
    {
        get;
        init;
    }
}

/// <summary>
///     Ordered selector chain.
/// </summary>
public sealed record GeneratePdfClickSelectorChain
{
    /// <summary>
    ///     Selectors clicked in order.
    /// </summary>
    public IReadOnlyList<string>? Selectors
    {
        get;
        init;
    }
}

/// <summary>
///     Basic authentication credentials for URL rendering.
/// </summary>
public sealed record GeneratePdfAuthentication
{
    /// <summary>
    ///     Username credential.
    /// </summary>
    public string? Username
    {
        get;
        init;
    }

    /// <summary>
    ///     Password credential.
    /// </summary>
    public string? Password
    {
        get;
        init;
    }
}

/// <summary>
///     Viewport dimensions used while rendering.
/// </summary>
public sealed record GeneratePdfViewport
{
    /// <summary>
    ///     Viewport width in pixels.
    /// </summary>
    public int? Width
    {
        get;
        init;
    }

    /// <summary>
    ///     Viewport height in pixels.
    /// </summary>
    public int? Height
    {
        get;
        init;
    }
}

/// <summary>
///     Supported page sizes for generated PDFs.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneratePdfPageSizeType
{
    /// <summary>
    ///     A0 page size.
    /// </summary>
    A0,

    /// <summary>
    ///     A1 page size.
    /// </summary>
    A1,

    /// <summary>
    ///     A2 page size.
    /// </summary>
    A2,

    /// <summary>
    ///     A3 page size.
    /// </summary>
    A3,

    /// <summary>
    ///     A4 page size.
    /// </summary>
    A4,

    /// <summary>
    ///     A5 page size.
    /// </summary>
    A5,

    /// <summary>
    ///     A6 page size.
    /// </summary>
    A6,

    /// <summary>
    ///     Ledger page size.
    /// </summary>
    Ledger,

    /// <summary>
    ///     Tabloid page size.
    /// </summary>
    Tabloid,

    /// <summary>
    ///     Legal page size.
    /// </summary>
    Legal,

    /// <summary>
    ///     Letter page size.
    /// </summary>
    Letter
}

/// <summary>
///     Supported orientation values for generated PDFs.
/// </summary>
public enum GeneratePdfFileOrientation
{
    /// <summary>
    ///     Portrait orientation.
    /// </summary>
    Portrait,

    /// <summary>
    ///     Landscape orientation.
    /// </summary>
    Landscape
}

/// <summary>
///     Supported media emulation values for rendering.
/// </summary>
public enum GeneratePdfEmulateMediaType
{
    /// <summary>
    ///     Screen media type.
    /// </summary>
    Screen,

    /// <summary>
    ///     Print media type.
    /// </summary>
    Print
}
