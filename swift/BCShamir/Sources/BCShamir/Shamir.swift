// BCShamir — Shamir's Secret Sharing for Swift
//
// Provides threshold-based secret splitting and recovery using Shamir's
// Secret Sharing scheme over GF(2^8). A secret is split into `n` shares
// such that any `t` of them (the threshold) can reconstruct the original
// secret, while fewer than `t` shares reveal no information about it.
//
// Secrets must be 16--32 bytes long (even length), and up to 16 shares
// can be generated.

import Foundation
import BCRand
import BCCrypto

/// The minimum length of a secret in bytes.
public let minSecretLen = 16

/// The maximum length of a secret in bytes.
public let maxSecretLen = 32

/// The maximum number of shares that can be generated from a secret.
public let maxShareCount = 16

private let secretIndex: UInt8 = 255
private let digestIndex: UInt8 = 254

private func createDigest(randomData: [UInt8], sharedSecret: [UInt8]) -> [UInt8] {
    Array(hmacSHA256(key: Data(randomData), message: Data(sharedSecret)))
}

private func validateParameters(threshold: Int, shareCount: Int, secretLength: Int) throws(ShamirError) {
    if shareCount > maxShareCount {
        throw ShamirError.tooManyShares
    } else if threshold < 1 || threshold > shareCount {
        throw ShamirError.invalidThreshold
    } else if secretLength > maxSecretLen {
        throw ShamirError.secretTooLong
    } else if secretLength < minSecretLen {
        throw ShamirError.secretTooShort
    } else if secretLength & 1 != 0 {
        throw ShamirError.secretLengthNotEven
    }
}

/// Splits a secret into shares using Shamir's Secret Sharing.
///
/// - Parameters:
///   - threshold: The minimum number of shares required to reconstruct the secret.
///   - shareCount: The total number of shares to generate.
///   - secret: The secret bytes to split (must be 16--32 bytes, even length).
///   - randomGenerator: A source of randomness conforming to ``BCRandomNumberGenerator``.
/// - Returns: An array of share byte arrays.
/// - Throws: ``ShamirError`` if parameters are invalid.
public func splitSecret(
    threshold: Int,
    shareCount: Int,
    secret: [UInt8],
    randomGenerator: inout some BCRandomNumberGenerator
) throws(ShamirError) -> [[UInt8]] {
    try validateParameters(threshold: threshold, shareCount: shareCount, secretLength: secret.count)

    if threshold == 1 {
        return [[UInt8]](repeating: secret, count: shareCount)
    } else {
        var x = [UInt8](repeating: 0, count: shareCount)
        var y = [[UInt8]](repeating: [UInt8](repeating: 0, count: secret.count), count: shareCount)
        var n = 0
        var result = [[UInt8]](repeating: [UInt8](repeating: 0, count: secret.count), count: shareCount)

        for index in 0..<(threshold - 2) {
            var randomData = Data(repeating: 0, count: secret.count)
            randomGenerator.fillRandomData(&randomData)
            result[index] = Array(randomData)
            x[n] = UInt8(index)
            y[n] = result[index]
            n += 1
        }

        // Generate secretLength - 4 bytes of random data for the digest padding.
        var digest = [UInt8](repeating: 0, count: secret.count)
        var digestData = Data(repeating: 0, count: secret.count - 4)
        randomGenerator.fillRandomData(&digestData)
        digest.replaceSubrange(4..<secret.count, with: Array(digestData))

        // Place the 4-byte HMAC digest prefix at the front.
        let d = createDigest(randomData: Array(digest[4...]), sharedSecret: secret)
        digest.replaceSubrange(0..<4, with: d[0..<4])
        x[n] = digestIndex
        y[n] = digest
        n += 1

        x[n] = secretIndex
        y[n] = secret
        n += 1

        for index in (threshold - 2)..<shareCount {
            let v = try interpolate(n: n, xi: x, yl: secret.count, yij: y, x: UInt8(index))
            result[index] = v
        }

        // Zero sensitive temporaries.
        memzero(&digest)
        memzero(&x)
        memzero(&y)

        return result
    }
}

/// Recovers a secret from the given shares using Shamir's Secret Sharing.
///
/// - Parameters:
///   - indices: The indices of the shares used for recovery.
///   - shares: The share byte arrays corresponding to the indices.
/// - Returns: The recovered secret bytes.
/// - Throws: ``ShamirError`` if parameters are invalid or the checksum fails.
public func recoverSecret(indices: [Int], shares: [[UInt8]]) throws(ShamirError) -> [UInt8] {
    let threshold = shares.count
    if threshold == 0 || indices.count != threshold {
        throw ShamirError.invalidThreshold
    }
    let shareLength = shares[0].count
    try validateParameters(threshold: threshold, shareCount: threshold, secretLength: shareLength)

    guard shares.allSatisfy({ $0.count == shareLength }) else {
        throw ShamirError.sharesUnequalLength
    }

    if threshold == 1 {
        return shares[0]
    } else {
        let byteIndices = indices.map { UInt8($0) }
        var digest = try interpolate(
            n: threshold,
            xi: byteIndices,
            yl: shareLength,
            yij: shares,
            x: digestIndex
        )
        let secret = try interpolate(
            n: threshold,
            xi: byteIndices,
            yl: shareLength,
            yij: shares,
            x: secretIndex
        )
        var verify = createDigest(randomData: Array(digest[4...]), sharedSecret: secret)

        var valid = true
        for i in 0..<4 {
            valid = valid && (digest[i] == verify[i])
        }
        memzero(&digest)
        memzero(&verify)

        if !valid {
            throw ShamirError.checksumFailure
        }

        return secret
    }
}
