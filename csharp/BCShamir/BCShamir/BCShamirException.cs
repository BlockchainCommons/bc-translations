namespace BlockchainCommons.BCShamir;

/// <summary>
/// Error codes produced by BCShamir operations.
/// </summary>
public enum Error
{
    SecretTooLong,
    TooManyShares,
    InterpolationFailure,
    ChecksumFailure,
    SecretTooShort,
    SecretNotEvenLen,
    InvalidThreshold,
    SharesUnequalLength,
}

/// <summary>
/// Exception thrown by BCShamir operations.
/// </summary>
public sealed class BCShamirException : Exception
{
    public Error Kind { get; }

    public BCShamirException(Error kind) : base(GetMessage(kind))
    {
        Kind = kind;
    }

    public BCShamirException(Error kind, string message) : base(message)
    {
        Kind = kind;
    }

    public BCShamirException(Error kind, string message, Exception innerException)
        : base(message, innerException)
    {
        Kind = kind;
    }

    private static string GetMessage(Error kind)
    {
        return kind switch
        {
            Error.SecretTooLong => "secret is too long",
            Error.TooManyShares => "too many shares",
            Error.InterpolationFailure => "interpolation failed",
            Error.ChecksumFailure => "checksum failure",
            Error.SecretTooShort => "secret is too short",
            Error.SecretNotEvenLen => "secret is not of even length",
            Error.InvalidThreshold => "invalid threshold",
            Error.SharesUnequalLength => "shares have unequal length",
            _ => "bc-shamir error"
        };
    }
}
