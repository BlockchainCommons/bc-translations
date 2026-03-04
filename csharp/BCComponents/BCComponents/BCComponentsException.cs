namespace BlockchainCommons.BCComponents;

/// <summary>
/// Exception type for bc-components errors.
/// Provides factory methods for common error categories such as invalid sizes,
/// invalid data, cryptographic failures, SSH errors, and more.
/// </summary>
public sealed class BCComponentsException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="BCComponentsException"/> with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public BCComponentsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="BCComponentsException"/> with the specified message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BCComponentsException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Creates an error for an invalid data size.</summary>
    /// <param name="dataType">The name of the data type.</param>
    /// <param name="expected">The expected size.</param>
    /// <param name="actual">The actual size received.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException InvalidSize(string dataType, int expected, int actual)
    {
        return new BCComponentsException($"invalid {dataType} size: expected {expected}, got {actual}");
    }

    /// <summary>Creates an error for invalid data content.</summary>
    /// <param name="dataType">The name of the data type.</param>
    /// <param name="reason">The reason the data is invalid.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException InvalidData(string dataType, string reason)
    {
        return new BCComponentsException($"invalid {dataType}: {reason}");
    }

    /// <summary>Creates an error when data is too short.</summary>
    /// <param name="dataType">The name of the data type.</param>
    /// <param name="minimum">The minimum required size.</param>
    /// <param name="actual">The actual size received.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException DataTooShort(string dataType, int minimum, int actual)
    {
        return new BCComponentsException($"data too short: {dataType} expected at least {minimum}, got {actual}");
    }

    /// <summary>Creates an error for a cryptographic operation failure.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException Crypto(string message)
    {
        return new BCComponentsException($"cryptographic operation failed: {message}");
    }

    /// <summary>Creates an error for an SSH operation failure.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException Ssh(string message)
    {
        return new BCComponentsException($"SSH operation failed: {message}");
    }

    /// <summary>Creates an error for a data compression/decompression failure.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException Compression(string message)
    {
        return new BCComponentsException($"compression error: {message}");
    }

    /// <summary>Creates an error for a post-quantum cryptography failure.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException PostQuantum(string message)
    {
        return new BCComponentsException($"post-quantum cryptography error: {message}");
    }

    /// <summary>Creates an error for an SSH agent operation failure.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException SshAgent(string message)
    {
        return new BCComponentsException($"SSH agent error: {message}");
    }

    /// <summary>Creates a general-purpose error with a custom message.</summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException General(string message)
    {
        return new BCComponentsException(message);
    }

    /// <summary>Creates an error for a signature level mismatch.</summary>
    /// <returns>A new <see cref="BCComponentsException"/>.</returns>
    public static BCComponentsException LevelMismatch()
    {
        return new BCComponentsException("signature level does not match key level");
    }
}
