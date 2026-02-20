import CryptoKit
import CryptoSwift
import Foundation

public let crc32Size = 4
public let sha256Size = 32
public let sha512Size = 64

public func crc32(_ data: Data) -> UInt32 {
    Array(data).crc32()
}

public func crc32DataOpt(_ data: Data, littleEndian: Bool) -> Data {
    let checksum = crc32(data)
    if littleEndian {
        return withUnsafeBytes(of: checksum.littleEndian) { Data($0) }
    }
    return withUnsafeBytes(of: checksum.bigEndian) { Data($0) }
}

public func crc32Data(_ data: Data) -> Data {
    crc32DataOpt(data, littleEndian: false)
}

public func sha256(_ data: Data) -> Data {
    Data(SHA256.hash(data: data))
}

public func doubleSHA256(_ message: Data) -> Data {
    sha256(sha256(message))
}

public func sha512(_ data: Data) -> Data {
    Data(SHA512.hash(data: data))
}

public func hmacSHA256(_ key: Data, _ message: Data) -> Data {
    let key = SymmetricKey(data: key)
    let code = HMAC<SHA256>.authenticationCode(for: message, using: key)
    return Data(code)
}

public func hmacSHA512(_ key: Data, _ message: Data) -> Data {
    let key = SymmetricKey(data: key)
    let code = HMAC<SHA512>.authenticationCode(for: message, using: key)
    return Data(code)
}

public func pbkdf2HmacSHA256(
    _ pass: Data,
    _ salt: Data,
    _ iterations: UInt32,
    _ keyLen: Int
) -> Data {
    let pbkdf2 = try! PKCS5.PBKDF2(
        password: Array(pass),
        salt: Array(salt),
        iterations: Int(iterations),
        keyLength: keyLen,
        variant: .sha2(.sha256)
    )
    return Data(try! pbkdf2.calculate())
}

public func pbkdf2HmacSHA512(
    _ pass: Data,
    _ salt: Data,
    _ iterations: UInt32,
    _ keyLen: Int
) -> Data {
    let pbkdf2 = try! PKCS5.PBKDF2(
        password: Array(pass),
        salt: Array(salt),
        iterations: Int(iterations),
        keyLength: keyLen,
        variant: .sha2(.sha512)
    )
    return Data(try! pbkdf2.calculate())
}

public func hkdfHmacSHA256(
    _ keyMaterial: Data,
    _ salt: Data,
    _ keyLen: Int
) -> Data {
    let key = HKDF<SHA256>.deriveKey(
        inputKeyMaterial: SymmetricKey(data: keyMaterial),
        salt: salt,
        info: Data(),
        outputByteCount: keyLen
    )
    return key.withUnsafeBytes { Data($0) }
}

public func hkdfHmacSHA512(
    _ keyMaterial: Data,
    _ salt: Data,
    _ keyLen: Int
) -> Data {
    let key = HKDF<SHA512>.deriveKey(
        inputKeyMaterial: SymmetricKey(data: keyMaterial),
        salt: salt,
        info: Data(),
        outputByteCount: keyLen
    )
    return key.withUnsafeBytes { Data($0) }
}
