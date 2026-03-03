using BlockchainCommons.BCShamir;

namespace BlockchainCommons.SSKR;

/// <summary>
/// Error codes produced by SSKR operations.
/// </summary>
public enum SskrError
{
    DuplicateMemberIndex,
    GroupSpecInvalid,
    GroupCountInvalid,
    GroupThresholdInvalid,
    MemberCountInvalid,
    MemberThresholdInvalid,
    NotEnoughGroups,
    SecretLengthNotEven,
    SecretTooLong,
    SecretTooShort,
    ShareLengthInvalid,
    ShareReservedBitsInvalid,
    SharesEmpty,
    ShareSetInvalid,
    ShamirError,
}

/// <summary>
/// Exception thrown by SSKR operations.
/// </summary>
public sealed class SSKRException : Exception
{
    /// <summary>The specific SSKR error that caused this exception.</summary>
    public SskrError ErrorKind { get; }

    /// <summary>
    /// The specific wrapped BCShamir error kind when <see cref="ErrorKind"/> is
    /// <see cref="SskrError.ShamirError"/>.
    /// </summary>
    public ShamirError? ShamirErrorKind { get; }

    public SSKRException(SskrError errorKind)
        : base(GetMessage(errorKind))
    {
        ErrorKind = errorKind;
    }

    public SSKRException(SskrError errorKind, string message)
        : base(message)
    {
        ErrorKind = errorKind;
    }

    public SSKRException(SskrError errorKind, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorKind = errorKind;
    }

    public SSKRException(BCShamirException innerException)
        : base(CreateShamirMessage(innerException), innerException)
    {
        ErrorKind = SskrError.ShamirError;
        ShamirErrorKind = innerException.ErrorKind;
    }

    private static string GetMessage(SskrError errorKind)
    {
        return errorKind switch
        {
            SskrError.DuplicateMemberIndex =>
                "When combining shares, the provided shares contained a duplicate member index",
            SskrError.GroupSpecInvalid => "Invalid group specification.",
            SskrError.GroupCountInvalid =>
                "When creating a split spec, the group count is invalid",
            SskrError.GroupThresholdInvalid => "SSKR group threshold is invalid",
            SskrError.MemberCountInvalid => "SSKR member count is invalid",
            SskrError.MemberThresholdInvalid => "SSKR member threshold is invalid",
            SskrError.NotEnoughGroups => "SSKR shares did not contain enough groups",
            SskrError.SecretLengthNotEven => "SSKR secret is not of even length",
            SskrError.SecretTooLong => "SSKR secret is too long",
            SskrError.SecretTooShort => "SSKR secret is too short",
            SskrError.ShareLengthInvalid =>
                "SSKR shares did not contain enough serialized bytes",
            SskrError.ShareReservedBitsInvalid =>
                "SSKR shares contained invalid reserved bits",
            SskrError.SharesEmpty => "SSKR shares were empty",
            SskrError.ShareSetInvalid => "SSKR shares were invalid",
            SskrError.ShamirError => "SSKR Shamir error",
            _ => "sskr error",
        };
    }

    private static string CreateShamirMessage(BCShamirException innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
        return $"SSKR Shamir error: {innerException.Message}";
    }
}
