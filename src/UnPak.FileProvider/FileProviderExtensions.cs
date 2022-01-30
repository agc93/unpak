namespace UnPak.FileProvider
{
    public static class FileProviderExtensions
    {
        internal static string StripPrefix(this string text, string prefix) {
            return text.StartsWith(prefix) ? text[prefix.Length..] : text;
        }
    }
}