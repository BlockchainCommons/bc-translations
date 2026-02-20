namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Exception thrown by BCCrypto operations.
/// </summary>
public class BCCryptoException : Exception
{
    public BCCryptoException(string message) : base(message) { }
    public BCCryptoException(string message, Exception innerException) : base(message, innerException) { }
}
