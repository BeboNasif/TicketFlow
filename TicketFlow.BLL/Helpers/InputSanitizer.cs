namespace TicketFlow.BLL.Helpers;

using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

public static class InputSanitizer
{
    private static readonly Regex DangerousTagRegex = new(
        @"<\s*(script|iframe|object|embed|form|style|svg|math|link|meta)[^>]*>.*?<\s*/\s*\1\s*>|<\s*(script|iframe|object|embed|form|style|svg|math|link|meta)[^>]*\/?>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex EventHandlerRegex = new(
        @"\bon\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string SanitizeAndEncode(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. Strip dangerous HTML tags and inline JS event handlers (e.g., onerror=, onclick=)
        var cleaned = DangerousTagRegex.Replace(input, string.Empty);
        cleaned = EventHandlerRegex.Replace(cleaned, string.Empty);

        // 2. HTML-encode remaining special characters (<, >, &, ", ')
        return HtmlEncoder.Default.Encode(cleaned.Trim());
    }

    public static bool ContainsMaliciousPayload(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return DangerousTagRegex.IsMatch(input)
            || EventHandlerRegex.IsMatch(input)
            || input.Contains("javascript:", StringComparison.OrdinalIgnoreCase);
    }
}