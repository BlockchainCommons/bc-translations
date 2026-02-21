namespace BlockchainCommons.BCShamir;

/// <summary>
/// Error codes produced by BCShamir operations.
/// </summary>
public enum ShamirError
{
    /// <summary>The secret exceeds <see cref="Shamir.MaxSecretLen"/> bytes.</summary>
    SecretTooLong,

    /// <summary>The requested share count exceeds <see cref="Shamir.MaxShareCount"/>.</summary>
    TooManyShares,

    /// <summary>Lagrange interpolation failed due to invalid inputs.</summary>
    InterpolationFailure,

    /// <summary>The recovered digest does not match the expected checksum.</summary>
    ChecksumFailure,

    /// <summary>The secret is shorter than <see cref="Shamir.MinSecretLen"/> bytes.</summary>
    SecretTooShort,

    /// <summary>The secret length is odd; an even byte length is required.</summary>
    SecretNotEvenLen,

    /// <summary>The threshold is out of range (must be 1..shareCount).</summary>
    InvalidThreshold,

    /// <summary>Not all shares have the same byte length.</summary>
    SharesUnequalLength,
}

/// <summary>
/// Exception thrown by BCShamir operations.
/// </summary>
public sealed class BCShamirException : Exception
{
    /// <summary>The specific error that caused this exception.</summary>
    public ShamirError ErrorKind { get; }

    public BCShamirException(ShamirError errorKind) : base(GetMessage(errorKind))
    {
        ErrorKind = errorKind;
    }

    public BCShamirException(ShamirError errorKind, string message) : base(message)
    {
        ErrorKind = errorKind;
    }

    public BCShamirException(ShamirError errorKind, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorKind = errorKind;
    }

    private static string GetMessage(ShamirError errorKind)
    {
        return errorKind switch
        {
            ShamirError.SecretTooLong => "secret is too long",
            ShamirError.TooManyShares => "too many shares",
            ShamirError.InterpolationFailure => "interpolation failed",
            ShamirError.ChecksumFailure => "checksum failure",
            ShamirError.SecretTooShort => "secret is too short",
            ShamirError.SecretNotEvenLen => "secret is not of even length",
            ShamirError.InvalidThreshold => "invalid threshold",
            ShamirError.SharesUnequalLength => "shares have unequal length",
            _ => "bc-shamir error"
        };
    }
}
