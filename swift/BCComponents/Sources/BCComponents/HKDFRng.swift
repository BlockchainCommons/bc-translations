import BCCrypto
import BCRand
import Foundation

/// Deterministic RNG based on HKDF-HMAC-SHA256.
public struct HKDFRng: BCRandomNumberGenerator {
    public private(set) var buffer = Data()
    public private(set) var position = 0
    public private(set) var keyMaterial: Data
    public private(set) var salt: String
    public private(set) var pageLength: Int
    public private(set) var pageIndex = 0

    public init(
        keyMaterial: some DataProtocol,
        salt: String,
        pageLength: Int
    ) {
        self.keyMaterial = Data(keyMaterial)
        self.salt = salt
        self.pageLength = pageLength
    }

    public init(keyMaterial: some DataProtocol, salt: String) {
        self.init(keyMaterial: keyMaterial, salt: salt, pageLength: 32)
    }

    private mutating func fillBuffer() {
        let saltString = "\(salt)-\(pageIndex)"
        buffer = hkdfHmacSHA256(
            keyMaterial: keyMaterial,
            salt: Data(saltString.utf8),
            keyLength: pageLength
        )
        position = 0
        pageIndex += 1
    }

    mutating func nextBytes(length: Int) -> Data {
        var result = Data()
        result.reserveCapacity(length)

        while result.count < length {
            if position >= buffer.count {
                fillBuffer()
            }

            let remaining = length - result.count
            let available = buffer.count - position
            let take = min(remaining, available)
            result.append(buffer[position..<(position + take)])
            position += take
        }

        return result
    }

    public mutating func nextUInt32() -> UInt32 {
        let b = [UInt8](nextBytes(length: 4))
        return UInt32(b[0])
            | (UInt32(b[1]) << 8)
            | (UInt32(b[2]) << 16)
            | (UInt32(b[3]) << 24)
    }

    public mutating func nextUInt64() -> UInt64 {
        let b = [UInt8](nextBytes(length: 8))
        return UInt64(b[0])
            | (UInt64(b[1]) << 8)
            | (UInt64(b[2]) << 16)
            | (UInt64(b[3]) << 24)
            | (UInt64(b[4]) << 32)
            | (UInt64(b[5]) << 40)
            | (UInt64(b[6]) << 48)
            | (UInt64(b[7]) << 56)
    }

    public mutating func fillRandomData(_ data: inout Data) {
        data = nextBytes(length: data.count)
    }
}
