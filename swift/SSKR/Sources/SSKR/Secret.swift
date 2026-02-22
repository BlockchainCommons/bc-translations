/// A secret to be split into shares.
public struct Secret: Equatable, Sendable {
    private let bytes: [UInt8]

    /// Creates a new `Secret` instance with the given data.
    ///
    /// - Parameter data: The secret data to be split into shares.
    /// - Throws: ``SSKRError`` if the length is less than ``minSecretLen``,
    ///   greater than ``maxSecretLen``, or not even.
    public init(_ data: [UInt8]) throws(SSKRError) {
        let len = data.count
        if len < minSecretLen {
            throw .secretTooShort
        }
        if len > maxSecretLen {
            throw .secretTooLong
        }
        if len & 1 != 0 {
            throw .secretLengthNotEven
        }
        self.bytes = data
    }

    /// The number of bytes in the secret.
    public var count: Int { bytes.count }

    /// Whether the secret is empty.
    public var isEmpty: Bool { bytes.isEmpty }

    /// The raw secret data.
    public var data: [UInt8] { bytes }
}
