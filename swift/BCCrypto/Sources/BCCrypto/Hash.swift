import CryptoKit
import CryptoSwift
import Foundation

/// The size in bytes of a CRC-32 checksum.
public let crc32Size = 4

/// The size in bytes of a SHA-256 digest.
public let sha256Size = 32

/// The size in bytes of a SHA-512 digest.
public let sha512Size = 64

/// Computes the CRC-32 checksum of the given data.
///
/// - Parameter data: The input data to checksum.
/// - Returns: The CRC-32 checksum as a 32-bit unsigned integer.
public func crc32(_ data: Data) -> UInt32 {
    Array(data).crc32()
}

/// Computes the CRC-32 checksum of the given data and returns it as raw bytes.
///
/// - Parameters:
///   - data: The input data to checksum.
///   - littleEndian: If `true`, returns the checksum in little-endian byte order;
///     otherwise big-endian.
/// - Returns: The CRC-32 checksum as a 4-byte `Data` value.
public func crc32Data(_ data: Data, littleEndian: Bool) -> Data {
    let checksum = crc32(data)
    if littleEndian {
        return withUnsafeBytes(of: checksum.littleEndian) { Data($0) }
    }
    return withUnsafeBytes(of: checksum.bigEndian) { Data($0) }
}

/// Computes the CRC-32 checksum of the given data in big-endian byte order.
///
/// - Parameter data: The input data to checksum.
/// - Returns: The CRC-32 checksum as a 4-byte `Data` value in big-endian order.
public func crc32Data(_ data: Data) -> Data {
    crc32Data(data, littleEndian: false)
}

/// Computes the SHA-256 digest of the given data.
///
/// - Parameter data: The input data to hash.
/// - Returns: The 32-byte SHA-256 digest.
public func sha256(_ data: Data) -> Data {
    Data(SHA256.hash(data: data))
}

/// Computes the double SHA-256 digest (SHA-256 applied twice) of the given message.
///
/// - Parameter message: The input data to hash.
/// - Returns: The 32-byte double SHA-256 digest.
public func doubleSHA256(_ message: Data) -> Data {
    sha256(sha256(message))
}

/// Computes the SHA-512 digest of the given data.
///
/// - Parameter data: The input data to hash.
/// - Returns: The 64-byte SHA-512 digest.
public func sha512(_ data: Data) -> Data {
    Data(SHA512.hash(data: data))
}

/// Computes an HMAC-SHA-256 authentication code.
///
/// - Parameters:
///   - key: The secret key.
///   - message: The message to authenticate.
/// - Returns: The 32-byte HMAC-SHA-256 code.
public func hmacSHA256(key: Data, message: Data) -> Data {
    let symmetricKey = SymmetricKey(data: key)
    let code = HMAC<SHA256>.authenticationCode(for: message, using: symmetricKey)
    return Data(code)
}

/// Computes an HMAC-SHA-512 authentication code.
///
/// - Parameters:
///   - key: The secret key.
///   - message: The message to authenticate.
/// - Returns: The 64-byte HMAC-SHA-512 code.
public func hmacSHA512(key: Data, message: Data) -> Data {
    let symmetricKey = SymmetricKey(data: key)
    let code = HMAC<SHA512>.authenticationCode(for: message, using: symmetricKey)
    return Data(code)
}

/// Derives a key using PBKDF2-HMAC-SHA-256.
///
/// - Parameters:
///   - password: The password bytes.
///   - salt: The salt bytes.
///   - iterations: The number of PBKDF2 iterations.
///   - keyLength: The desired output key length in bytes.
/// - Returns: The derived key.
public func pbkdf2HmacSHA256(
    password: Data,
    salt: Data,
    iterations: UInt32,
    keyLength: Int
) -> Data {
    let pbkdf2 = try! PKCS5.PBKDF2(
        password: Array(password),
        salt: Array(salt),
        iterations: Int(iterations),
        keyLength: keyLength,
        variant: .sha2(.sha256)
    )
    return Data(try! pbkdf2.calculate())
}

/// Derives a key using PBKDF2-HMAC-SHA-512.
///
/// - Parameters:
///   - password: The password bytes.
///   - salt: The salt bytes.
///   - iterations: The number of PBKDF2 iterations.
///   - keyLength: The desired output key length in bytes.
/// - Returns: The derived key.
public func pbkdf2HmacSHA512(
    password: Data,
    salt: Data,
    iterations: UInt32,
    keyLength: Int
) -> Data {
    let pbkdf2 = try! PKCS5.PBKDF2(
        password: Array(password),
        salt: Array(salt),
        iterations: Int(iterations),
        keyLength: keyLength,
        variant: .sha2(.sha512)
    )
    return Data(try! pbkdf2.calculate())
}

/// Derives a key using HKDF-HMAC-SHA-256.
///
/// - Parameters:
///   - keyMaterial: The input key material.
///   - salt: The salt value.
///   - keyLength: The desired output key length in bytes.
/// - Returns: The derived key.
public func hkdfHmacSHA256(
    keyMaterial: Data,
    salt: Data,
    keyLength: Int
) -> Data {
    let key = HKDF<SHA256>.deriveKey(
        inputKeyMaterial: SymmetricKey(data: keyMaterial),
        salt: salt,
        info: Data(),
        outputByteCount: keyLength
    )
    return key.withUnsafeBytes { Data($0) }
}

/// Derives a key using HKDF-HMAC-SHA-512.
///
/// - Parameters:
///   - keyMaterial: The input key material.
///   - salt: The salt value.
///   - keyLength: The desired output key length in bytes.
/// - Returns: The derived key.
public func hkdfHmacSHA512(
    keyMaterial: Data,
    salt: Data,
    keyLength: Int
) -> Data {
    let key = HKDF<SHA512>.deriveKey(
        inputKeyMaterial: SymmetricKey(data: keyMaterial),
        salt: salt,
        info: Data(),
        outputByteCount: keyLength
    )
    return key.withUnsafeBytes { Data($0) }
}
