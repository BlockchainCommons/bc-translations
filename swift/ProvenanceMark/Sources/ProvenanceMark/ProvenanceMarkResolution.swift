import Foundation
import DCBOR

/// The resolution of a provenance mark, determining the length of cryptographic
/// links and the precision of embedded date and sequence fields.
public enum ProvenanceMarkResolution: UInt8, Sendable, Codable, Hashable, CaseIterable {
    case low      = 0
    case medium   = 1
    case quartile = 2
    case high     = 3
}

// MARK: - Layout geometry

public extension ProvenanceMarkResolution {
    /// The byte length of each cryptographic link (key, hash, chain ID).
    var linkLength: Int {
        switch self {
        case .low:      return 4
        case .medium:   return 8
        case .quartile: return 16
        case .high:     return 32
        }
    }

    /// The byte length of the serialized sequence number.
    var seqBytesLength: Int {
        switch self {
        case .low:
            return 2
        case .medium, .quartile, .high:
            return 4
        }
    }

    /// The byte length of the serialized date.
    var dateBytesLength: Int {
        switch self {
        case .low:
            return 2
        case .medium:
            return 4
        case .quartile, .high:
            return 6
        }
    }

    /// The total fixed-length portion of a serialized provenance mark
    /// (key + hash + chainID + seq + date).
    var fixedLength: Int {
        linkLength * 3 + seqBytesLength + dateBytesLength
    }

    // MARK: Byte ranges within the key block

    /// The range of the key within the key block.
    var keyRange: Range<Int> {
        0..<linkLength
    }

    // MARK: Byte ranges within the payload (after key)

    /// The range of the chain ID within the payload.
    var chainIdRange: Range<Int> {
        0..<linkLength
    }

    /// The range of the hash within the payload.
    var hashRange: Range<Int> {
        chainIdRange.upperBound..<(chainIdRange.upperBound + linkLength)
    }

    /// The range of the sequence-number bytes within the payload.
    var seqBytesRange: Range<Int> {
        hashRange.upperBound..<(hashRange.upperBound + seqBytesLength)
    }

    /// The range of the date bytes within the payload.
    var dateBytesRange: Range<Int> {
        seqBytesRange.upperBound..<(seqBytesRange.upperBound + dateBytesLength)
    }

    /// The starting index of the variable-length info region within the payload.
    var infoRangeStart: Int {
        dateBytesRange.upperBound
    }
}

// MARK: - Date serialization helpers

public extension ProvenanceMarkResolution {
    /// Serializes a `Date` into bytes at this resolution's date precision.
    func serializeDate(_ date: Date) throws -> [UInt8] {
        switch self {
        case .low:
            return try date.serialize2Bytes()
        case .medium:
            return try date.serialize4Bytes()
        case .quartile, .high:
            return try date.serialize6Bytes()
        }
    }

    /// Deserializes bytes into a `Date` at this resolution's date precision.
    func deserializeDate(_ data: [UInt8]) throws -> Date {
        switch self {
        case .low:
            guard data.count == 2 else {
                throw ProvenanceMarkError.resolutionError(
                    details: "invalid date length: expected 2 bytes, got \(data.count)")
            }
            return try Date.deserialize2Bytes(data)
        case .medium:
            guard data.count == 4 else {
                throw ProvenanceMarkError.resolutionError(
                    details: "invalid date length: expected 4 bytes, got \(data.count)")
            }
            return try Date.deserialize4Bytes(data)
        case .quartile, .high:
            guard data.count == 6 else {
                throw ProvenanceMarkError.resolutionError(
                    details: "invalid date length: expected 6 bytes, got \(data.count)")
            }
            return try Date.deserialize6Bytes(data)
        }
    }
}

// MARK: - Sequence serialization helpers

public extension ProvenanceMarkResolution {
    /// Serializes a sequence number into big-endian bytes at this resolution's
    /// sequence precision (2 bytes for low, 4 bytes otherwise).
    func serializeSeq(_ seq: UInt32) throws -> [UInt8] {
        switch seqBytesLength {
        case 2:
            guard seq <= UInt16.max else {
                throw ProvenanceMarkError.resolutionError(
                    details: "sequence number \(seq) out of range for 2-byte format (max \(UInt16.max))")
            }
            let value = UInt16(seq)
            return [UInt8(value >> 8), UInt8(value & 0xFF)]
        case 4:
            return [
                UInt8((seq >> 24) & 0xFF),
                UInt8((seq >> 16) & 0xFF),
                UInt8((seq >> 8) & 0xFF),
                UInt8(seq & 0xFF),
            ]
        default:
            fatalError("unreachable")
        }
    }

    /// Deserializes big-endian bytes into a sequence number.
    func deserializeSeq(_ data: [UInt8]) throws -> UInt32 {
        switch seqBytesLength {
        case 2:
            guard data.count == 2 else {
                throw ProvenanceMarkError.resolutionError(
                    details: "invalid sequence number length: expected 2 bytes, got \(data.count)")
            }
            return UInt32(data[0]) << 8 | UInt32(data[1])
        case 4:
            guard data.count == 4 else {
                throw ProvenanceMarkError.resolutionError(
                    details: "invalid sequence number length: expected 4 bytes, got \(data.count)")
            }
            return UInt32(data[0]) << 24
                | UInt32(data[1]) << 16
                | UInt32(data[2]) << 8
                | UInt32(data[3])
        default:
            fatalError("unreachable")
        }
    }
}

// MARK: - CBOR

extension ProvenanceMarkResolution: CBORCodable {
    public var cbor: CBOR {
        CBOR(rawValue)
    }

    public init(cbor: CBOR) throws {
        let value = try UInt8(cbor: cbor)
        guard let resolution = ProvenanceMarkResolution(rawValue: value) else {
            throw ProvenanceMarkError.resolutionError(
                details: "invalid provenance mark resolution value: \(value)")
        }
        self = resolution
    }
}

// MARK: - CustomStringConvertible

extension ProvenanceMarkResolution: CustomStringConvertible {
    public var description: String {
        switch self {
        case .low:      return "low"
        case .medium:   return "medium"
        case .quartile: return "quartile"
        case .high:     return "high"
        }
    }
}
