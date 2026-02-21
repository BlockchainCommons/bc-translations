import Foundation
import Sodium

/// Derives a key using Argon2id password hashing.
///
/// The salt is normalized to `crypto_pwhash_SALTBYTES` by zero-padding or truncation.
///
/// - Parameters:
///   - password: The password bytes.
///   - salt: The salt bytes (will be padded or truncated to the required length).
///   - outputLength: The desired output length in bytes.
/// - Returns: The derived key.
public func argon2id(password: Data, salt: Data, outputLength: Int) -> Data {
    let sodium = Sodium()
    let saltBytes = sodium.pwHash.SaltBytes
    var normalizedSalt = Array(salt)

    if normalizedSalt.count < saltBytes {
        normalizedSalt.append(
            contentsOf: repeatElement(0, count: saltBytes - normalizedSalt.count)
        )
    } else if normalizedSalt.count > saltBytes {
        normalizedSalt = Array(normalizedSalt.prefix(saltBytes))
    }

    let hashed = sodium.pwHash.hash(
        outputLength: outputLength,
        passwd: Array(password),
        salt: normalizedSalt,
        opsLimit: 2,
        memLimit: 19 * 1024 * 1024,
        alg: .Argon2ID13
    )!

    return Data(hashed)
}
