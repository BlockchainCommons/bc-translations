import Foundation
import DCBOR

/// A 32-byte snapshot of the Xoshiro256** generator state, used for
/// serializing and restoring a `ProvenanceMarkGenerator`.
public struct RngState: Sendable, Equatable {
    /// The byte length of an RNG state snapshot.
    public static let byteLength = 32

    /// The raw bytes of the RNG state.
    public let bytes: [UInt8]

    // MARK: - Initializers

    /// Creates an RNG state from exactly ``byteLength`` bytes.
    public init(bytes: [UInt8]) {
        precondition(bytes.count == Self.byteLength)
        self.bytes = bytes
    }

    /// Creates an RNG state from a byte slice, validating the length.
    ///
    /// - Throws: An error if the slice is not exactly 32 bytes.
    public init(slice: [UInt8]) throws {
        guard slice.count == Self.byteLength else {
            throw ProvenanceMarkError.invalidSeedLength(actual: slice.count)
        }
        self.bytes = slice
    }

    // MARK: - Accessors

    /// The lowercase hex representation of the state bytes.
    public var hex: String {
        bytes.map { String(format: "%02x", $0) }.joined()
    }
}

// MARK: - CBOR

extension RngState: CBORCodable {
    public var cbor: CBOR {
        .bytes(Data(bytes))
    }

    public init(cbor: CBOR) throws {
        let data = try Data(cbor: cbor)
        try self.init(slice: [UInt8](data))
    }
}

// MARK: - Codable (base64 string representation)

extension RngState: Codable {
    public func encode(to encoder: Encoder) throws {
        var container = encoder.singleValueContainer()
        try container.encode(Data(bytes).base64EncodedString())
    }

    public init(from decoder: Decoder) throws {
        let container = try decoder.singleValueContainer()
        let base64String = try container.decode(String.self)
        guard let data = Data(base64Encoded: base64String) else {
            throw DecodingError.dataCorruptedError(
                in: container,
                debugDescription: "Invalid base64 string for RngState")
        }
        guard data.count == Self.byteLength else {
            throw DecodingError.dataCorruptedError(
                in: container,
                debugDescription: "RNG state length is \(data.count), expected \(Self.byteLength)")
        }
        self.bytes = [UInt8](data)
    }
}
