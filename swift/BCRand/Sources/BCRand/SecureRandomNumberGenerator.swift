import Foundation
import Security

/// A random number generator backed by the OS cryptographic random source.
///
/// All methods are thread-safe since `SecRandomCopyBytes` is thread-safe.
public struct SecureRandomNumberGenerator: BCRandomNumberGenerator, Sendable {
    public init() {}

    public func nextUInt32() -> UInt32 {
        UInt32(truncatingIfNeeded: Self.nextUInt64Value())
    }

    public func nextUInt64() -> UInt64 {
        Self.nextUInt64Value()
    }

    public func randomData(count: Int) -> Data {
        BCRand.randomData(count: count)
    }

    public func fillRandomData(_ data: inout Data) {
        BCRand.fillRandomData(&data)
    }

    private static func nextUInt64Value() -> UInt64 {
        var value: UInt64 = 0
        withUnsafeMutableBytes(of: &value) { buffer in
            let status = SecRandomCopyBytes(kSecRandomDefault, 8, buffer.baseAddress!)
            guard status == errSecSuccess else {
                fatalError("SecRandomCopyBytes failed with status \(status)")
            }
        }
        return value
    }
}

/// Returns cryptographically strong random bytes.
public func randomData(count: Int) -> Data {
    var data = Data(count: count)
    fillRandomData(&data)
    return data
}

/// Fills the given data with cryptographically strong random bytes.
public func fillRandomData(_ data: inout Data) {
    data.withUnsafeMutableBytes { buffer in
        guard let ptr = buffer.baseAddress else { return }
        let status = SecRandomCopyBytes(kSecRandomDefault, buffer.count, ptr)
        guard status == errSecSuccess else {
            fatalError("SecRandomCopyBytes failed with status \(status)")
        }
    }
}
