import Foundation
import DCBOR
import BCRand

/// A 32-byte seed used to initialize a provenance mark chain.
///
/// The seed can be created from secure randomness, a deterministic RNG (for
/// testing), or a passphrase (via HKDF key extension).
public struct ProvenanceSeed: Sendable, Equatable {
    /// The byte length of a provenance seed.
    public static let byteLength = 32

    /// The raw bytes of the seed.
    public let bytes: [UInt8]

    // MARK: - Initializers

    /// Creates a new seed from cryptographically secure randomness.
    public init() {
        var rng = SecureRandomNumberGenerator()
        self.init(using: &rng)
    }

    /// Creates a new seed from the given random number generator.
    public init(using rng: inout some BCRandomNumberGenerator) {
        let data = rngRandomData(&rng, count: Self.byteLength)
        self.bytes = [UInt8](data)
    }

    /// Creates a seed derived from a passphrase via HKDF key extension.
    public init(passphrase: String) {
        let extended = CryptoUtils.extendKey(Array(passphrase.utf8))
        self.bytes = extended
    }

    /// Creates a seed from exactly ``byteLength`` bytes.
    public init(bytes: [UInt8]) {
        precondition(bytes.count == Self.byteLength)
        self.bytes = bytes
    }

    /// Creates a seed from a byte slice, validating the length.
    ///
    /// - Throws: `ProvenanceMarkError.invalidSeedLength` if the slice is not
    ///   exactly 32 bytes.
    public init(slice: [UInt8]) throws {
        guard slice.count == Self.byteLength else {
            throw ProvenanceMarkError.invalidSeedLength(actual: slice.count)
        }
        self.bytes = slice
    }

    // MARK: - Accessors

    /// The lowercase hex representation of the seed bytes.
    public var hex: String {
        bytes.map { String(format: "%02x", $0) }.joined()
    }
}

// MARK: - CBOR

extension ProvenanceSeed: CBORCodable {
    public var cbor: CBOR {
        .bytes(Data(bytes))
    }

    public init(cbor: CBOR) throws {
        let data = try Data(cbor: cbor)
        try self.init(slice: [UInt8](data))
    }
}

// MARK: - Codable (base64 string representation)

extension ProvenanceSeed: Codable {
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
                debugDescription: "Invalid base64 string for ProvenanceSeed")
        }
        guard data.count == Self.byteLength else {
            throw DecodingError.dataCorruptedError(
                in: container,
                debugDescription: "seed length is \(data.count), expected \(Self.byteLength)")
        }
        self.bytes = [UInt8](data)
    }
}
