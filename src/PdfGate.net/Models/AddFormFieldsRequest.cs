namespace PdfGate.net.Models;

/// <summary>
///     Request payload used for the add form fields endpoint.
/// </summary>
/// <remarks>
///     Two complementary ways to add fields: customize placeholder fields detected in the PDF
///     via <see cref="FieldOverrides" /> (keyed by field name), and/or place fields at explicit
///     positions via <see cref="Fields" />. PDFGate creates a new document with the added fields.
/// </remarks>
public sealed record AddFormFieldsRequest
{
    /// <summary>
    ///     Existing stored PDF document ID.
    /// </summary>
    public string? DocumentId
    {
        get;
        init;
    }

    /// <summary>
    ///     Overrides for placeholder fields detected in the PDF, keyed by field name.
    /// </summary>
    public IReadOnlyDictionary<string, FieldOverride>? FieldOverrides
    {
        get;
        init;
    }

    /// <summary>
    ///     Fields to add at explicit positions on the document.
    /// </summary>
    public IReadOnlyList<ManualFormField>? Fields
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
    ///     Custom metadata attached to the resulting document.
    /// </summary>
    public object? Metadata
    {
        get;
        init;
    }
}

/// <summary>
///     Overrides applied to a placeholder field detected in the PDF.
/// </summary>
public sealed record FieldOverride
{
    /// <summary>
    ///     Select options for the field.
    /// </summary>
    public IReadOnlyList<string>? Options
    {
        get;
        init;
    }

    /// <summary>
    ///     Field height.
    /// </summary>
    public int? Height
    {
        get;
        init;
    }

    /// <summary>
    ///     Field width.
    /// </summary>
    public int? Width
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipient role assigned to the field.
    /// </summary>
    public string? Role
    {
        get;
        init;
    }

    /// <summary>
    ///     Font size.
    /// </summary>
    public int? FontSize
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the field is auto-filled.
    /// </summary>
    public bool? AutoFill
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the field is optional.
    /// </summary>
    public bool? Optional
    {
        get;
        init;
    }

    /// <summary>
    ///     Field description.
    /// </summary>
    public string? Description
    {
        get;
        init;
    }
}

/// <summary>
///     A form field placed at an explicit position on a given page.
/// </summary>
public sealed record ManualFormField
{
    /// <summary>
    ///     Field name.
    /// </summary>
    public required string Name
    {
        get;
        init;
    }

    /// <summary>
    ///     Field type.
    /// </summary>
    public required DocumentFieldType Type
    {
        get;
        init;
    }

    /// <summary>
    ///     1-based page number.
    /// </summary>
    public required int Page
    {
        get;
        init;
    }

    /// <summary>
    ///     Field height.
    /// </summary>
    public required int Height
    {
        get;
        init;
    }

    /// <summary>
    ///     Field width.
    /// </summary>
    public required int Width
    {
        get;
        init;
    }

    /// <summary>
    ///     X coordinate.
    /// </summary>
    public required double X
    {
        get;
        init;
    }

    /// <summary>
    ///     Y coordinate.
    /// </summary>
    public required double Y
    {
        get;
        init;
    }

    /// <summary>
    ///     Field value.
    /// </summary>
    public string? Value
    {
        get;
        init;
    }

    /// <summary>
    ///     Select options for the field.
    /// </summary>
    public IReadOnlyList<string>? Options
    {
        get;
        init;
    }

    /// <summary>
    ///     Recipient role assigned to the field.
    /// </summary>
    public string? Role
    {
        get;
        init;
    }

    /// <summary>
    ///     Font size.
    /// </summary>
    public int? FontSize
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the field is auto-filled.
    /// </summary>
    public bool? AutoFill
    {
        get;
        init;
    }

    /// <summary>
    ///     Whether the field is optional.
    /// </summary>
    public bool? Optional
    {
        get;
        init;
    }

    /// <summary>
    ///     Field description.
    /// </summary>
    public string? Description
    {
        get;
        init;
    }
}
