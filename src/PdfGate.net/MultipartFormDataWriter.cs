using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using PdfGate.net.Models;

namespace PdfGate.net;

/// <summary>
///     Writes multipart form fields and files in a consistent format.
/// </summary>
internal sealed class MultipartFormDataWriter(JsonSerializerOptions jsonOptions)
{
    /// <summary>
    ///     Adds a required string part to a multipart payload.
    /// </summary>
    public void AddString(
        MultipartFormDataContent form,
        string value,
        string partName)
    {
        form.Add(new StringContent(value, Encoding.UTF8), partName);
    }

    /// <summary>
    ///     Adds a string part to a multipart payload when the value is not null.
    /// </summary>
    public void AddStringIfNotNull(
        MultipartFormDataContent form,
        string? value,
        string partName)
    {
        if (value is null)
            return;

        AddString(form, value, partName);
    }

    /// <summary>
    ///     Adds a file part to a multipart payload when the file is not null.
    /// </summary>
    public void AddFileIfNotNull(
        MultipartFormDataContent form,
        PdfGateFile? file,
        string partName)
    {
        if (file is null)
            return;

        var fileContent = new StreamContent(file.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.Type);
        form.Add(fileContent, partName, file.Name);
    }

    /// <summary>
    ///     Adds a file part from a stream to a multipart payload.
    /// </summary>
    public void AddStreamFile(
        MultipartFormDataContent form,
        Stream content,
        string partName,
        string fileName,
        string mediaType)
    {
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        form.Add(fileContent, partName, fileName);
    }

    /// <summary>
    ///     Adds a JSON-serialized part to a multipart payload when the value is not null.
    /// </summary>
    public void AddJsonIfNotNull(
        MultipartFormDataContent form,
        object? value,
        string partName)
    {
        if (value is null)
            return;

        AddString(form, JsonSerializer.Serialize(value, jsonOptions), partName);
    }
}
