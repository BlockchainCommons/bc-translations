namespace BlockchainCommons.DCbor;

/// <summary>
/// Internal string utility functions for diagnostic formatting.
/// </summary>
internal static class StringUtil
{
    internal static string Flanked(string s, string left, string right)
    {
        return left + s + right;
    }

    internal static bool IsPrintable(char c)
    {
        return !char.IsAscii(c) || (c >= 32 && c <= 126);
    }

    internal static string? Sanitized(string s)
    {
        bool hasPrintable = false;
        var chars = new char[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            if (IsPrintable(s[i]))
            {
                hasPrintable = true;
                chars[i] = s[i];
            }
            else
            {
                chars[i] = '.';
            }
        }
        return hasPrintable ? new string(chars) : null;
    }
}
