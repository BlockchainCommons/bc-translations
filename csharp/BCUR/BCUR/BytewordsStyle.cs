namespace BlockchainCommons.BCUR;

/// <summary>
/// The three different bytewords encoding styles.
/// </summary>
public enum BytewordsStyle
{
    /// <summary>Four-letter words, separated by spaces.</summary>
    Standard,
    /// <summary>Four-letter words, separated by dashes.</summary>
    Uri,
    /// <summary>Two-letter words (first and last letter), concatenated without separators.</summary>
    Minimal
}
