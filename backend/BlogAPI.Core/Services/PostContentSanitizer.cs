using Ganss.Xss;

namespace BlogAPI.Core.Services;

/// <summary>
/// Strips dangerous markup (script tags, event handler attributes like onload,
/// javascript: URLs, etc.) from post content before it is persisted. Needed
/// because content is rendered both via [innerHTML] on the read view and loaded
/// directly into the Quill editor's DOM on the edit view - the latter isn't
/// covered by Angular's built-in sanitizer.
/// </summary>
internal static class PostContentSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = new();

    public static string Sanitize(string html) => Sanitizer.Sanitize(html);
}
