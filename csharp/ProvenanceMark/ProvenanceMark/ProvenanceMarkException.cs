namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Base exception type for provenance-mark failures.
/// </summary>
public class ProvenanceMarkException : Exception
{
    public ProvenanceMarkException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public static ProvenanceMarkException InvalidSeedLength(int actual) =>
        new($"invalid seed length: expected 32 bytes, got {actual} bytes");

    public static ProvenanceMarkException DuplicateKey(string key) =>
        new($"duplicate key: {key}");

    public static ProvenanceMarkException MissingKey(string key) =>
        new($"missing key: {key}");

    public static ProvenanceMarkException InvalidKey(string key) =>
        new($"invalid key: {key}");

    public static ProvenanceMarkException ExtraKeys(int expected, int actual) =>
        new($"wrong number of keys: expected {expected}, got {actual}");

    public static ProvenanceMarkException InvalidKeyLength(int expected, int actual) =>
        new($"invalid key length: expected {expected}, got {actual}");

    public static ProvenanceMarkException InvalidNextKeyLength(int expected, int actual) =>
        new($"invalid next key length: expected {expected}, got {actual}");

    public static ProvenanceMarkException InvalidChainIdLength(int expected, int actual) =>
        new($"invalid chain ID length: expected {expected}, got {actual}");

    public static ProvenanceMarkException InvalidMessageLength(int expected, int actual) =>
        new($"invalid message length: expected at least {expected}, got {actual}");

    public static ProvenanceMarkException InvalidInfoCbor() =>
        new("invalid CBOR data in info field");

    public static ProvenanceMarkException DateOutOfRange(string details) =>
        new($"date out of range: {details}");

    public static ProvenanceMarkException InvalidDate(string details) =>
        new($"invalid date: {details}");

    public static ProvenanceMarkException MissingUrlParameter(string parameter) =>
        new($"missing required URL parameter: {parameter}");

    public static ProvenanceMarkException YearOutOfRange(int year) =>
        new($"year out of range for 2-byte serialization: must be between 2023-2150, got {year}");

    public static ProvenanceMarkException InvalidMonthOrDay(int year, int month, int day) =>
        new($"invalid month ({month}) or day ({day}) for year {year}");

    public static ProvenanceMarkException ResolutionError(string details) =>
        new($"resolution serialization error: {details}");

    public static ProvenanceMarkException Bytewords(string details, Exception? innerException = null) =>
        new($"bytewords error: {details}", innerException);

    public static ProvenanceMarkException Cbor(string details, Exception? innerException = null) =>
        new($"CBOR error: {details}", innerException);

    public static ProvenanceMarkException Url(string details, Exception? innerException = null) =>
        new($"URL parsing error: {details}", innerException);

    public static ProvenanceMarkException Base64(string details, Exception? innerException = null) =>
        new($"base64 decoding error: {details}", innerException);

    public static ProvenanceMarkException Json(string details, Exception? innerException = null) =>
        new($"JSON error: {details}", innerException);

    public static ProvenanceMarkException IntegerConversion(string details) =>
        new($"integer conversion error: {details}");

    public static ProvenanceMarkException Envelope(string details, Exception? innerException = null) =>
        new($"envelope error: {details}", innerException);
}

/// <summary>
/// Validation-specific exception that preserves the flagged issue.
/// </summary>
public sealed class ProvenanceMarkValidationException : ProvenanceMarkException
{
    public ProvenanceMarkValidationException(ValidationIssue issue)
        : base($"validation error: {issue}")
    {
        Issue = issue;
    }

    public ValidationIssue Issue { get; }
}
