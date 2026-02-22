/// The three different bytewords encoding styles.
public enum BytewordsStyle: Sendable {
    /// Four-letter words, separated by spaces.
    case standard

    /// Four-letter words, separated by dashes.
    case uri

    /// Two-letter words, concatenated without separators.
    case minimal
}
