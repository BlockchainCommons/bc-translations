/// Errors that can occur during BCCrypto operations.
public enum BCCryptoError: Error, Equatable, Sendable {
    /// Authentication tag verification failed during AEAD decryption.
    case authenticationFailed
}
