namespace PheasantTails.TwiHigh.BlazorApp.Client.Extensions;

public static class StringExtension
{
    public static string GetTextWithNewline(this string text)
        => text.TrimStart('\r', '\n').TrimEnd('\r', '\n').Replace(Environment.NewLine, "<br />");
}
