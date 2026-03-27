namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Extension methods for string flanking operations.
/// </summary>
internal static class StringUtils
{
    /// <summary>
    /// Returns the string flanked by the given left and right delimiters.
    /// </summary>
    public static string FlankedBy(this string s, string left, string right)
    {
        return left + s + right;
    }
}
