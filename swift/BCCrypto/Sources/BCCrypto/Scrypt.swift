import CryptoSwift
import Foundation

/// Derives a key using scrypt with the default parameters (logN=15, r=8, p=1).
///
/// - Parameters:
///   - password: The password bytes.
///   - salt: The salt bytes.
///   - outputLength: The desired output length in bytes.
/// - Returns: The derived key.
public func scrypt(password: Data, salt: Data, outputLength: Int) -> Data {
    scrypt(password: password, salt: salt, outputLength: outputLength, logN: 15, r: 8, p: 1)
}

/// Derives a key using scrypt with custom parameters.
///
/// - Parameters:
///   - password: The password bytes.
///   - salt: The salt bytes.
///   - outputLength: The desired output length in bytes.
///   - logN: The log2 of the CPU/memory cost parameter N.
///   - r: The block size parameter.
///   - p: The parallelism parameter.
/// - Returns: The derived key.
public func scrypt(
    password: Data,
    salt: Data,
    outputLength: Int,
    logN: UInt8,
    r: UInt32,
    p: UInt32
) -> Data {
    let scryptKDF = try! Scrypt(
        password: Array(password),
        salt: Array(salt),
        dkLen: outputLength,
        N: 1 << Int(logN),
        r: Int(r),
        p: Int(p)
    )
    return Data(try! scryptKDF.calculate())
}
