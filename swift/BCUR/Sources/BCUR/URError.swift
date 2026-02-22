import Foundation

/// Errors that can occur in BCUR operations.
public enum URError: Error, Equatable, Sendable {
    /// An error produced by the internal UR codec layer.
    case ur(String)

    /// An error produced by bytewords encoding/decoding.
    case bytewords(String)

    /// An error produced by CBOR encode/decode.
    case cbor(String)

    /// The string did not start with the `ur:` scheme.
    case invalidScheme

    /// The UR type segment was missing.
    case typeUnspecified

    /// The UR type was invalid.
    case invalidType

    /// A single-part UR was expected but another kind was provided.
    case notSinglePart

    /// A specific UR type was expected but a different type was found.
    case unexpectedType(String, String)
}

extension URError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .ur(let message):
            return "UR decoder error (\(message))"
        case .bytewords(let message):
            return "Bytewords error (\(message))"
        case .cbor(let message):
            return "CBOR error (\(message))"
        case .invalidScheme:
            return "invalid UR scheme"
        case .typeUnspecified:
            return "no UR type specified"
        case .invalidType:
            return "invalid UR type"
        case .notSinglePart:
            return "UR is not a single-part"
        case .unexpectedType(let expected, let found):
            return "expected UR type \(expected), but found \(found)"
        }
    }
}

extension URError {
    init(cborError error: Error) {
        let message = (error as? LocalizedError)?.errorDescription ?? String(describing: error)
        self = .cbor(message)
    }

    init(bytewords error: BytewordsCodecError) {
        self = .bytewords(error.description)
    }

    init(ur error: URCodecError) {
        self = .ur(error.description)
    }

    init(fountain error: FountainError) {
        self = .ur(error.description)
    }
}

/// Internal bytewords codec errors (Rust `ur::bytewords::Error`).
internal enum BytewordsCodecError: Error, Equatable, Sendable {
    case invalidWord
    case invalidChecksum
    case invalidLength
    case nonAscii
}

extension BytewordsCodecError: CustomStringConvertible {
    var description: String {
        switch self {
        case .invalidWord:
            return "invalid word"
        case .invalidChecksum:
            return "invalid checksum"
        case .invalidLength:
            return "invalid length"
        case .nonAscii:
            return "bytewords string contains non-ASCII characters"
        }
    }
}

/// Internal UR codec errors (Rust `ur::ur::Error`).
internal enum URCodecError: Error, Equatable, Sendable {
    case bytewords(BytewordsCodecError)
    case fountain(FountainError)
    case invalidScheme
    case typeUnspecified
    case invalidCharacters
    case invalidIndices
    case notMultiPart
}

extension URCodecError: CustomStringConvertible {
    var description: String {
        switch self {
        case .bytewords(let error):
            return error.description
        case .fountain(let error):
            return error.description
        case .invalidScheme:
            return "Invalid scheme"
        case .typeUnspecified:
            return "No type specified"
        case .invalidCharacters:
            return "Type contains invalid characters"
        case .invalidIndices:
            return "Invalid indices"
        case .notMultiPart:
            return "Can't decode single-part UR as multi-part"
        }
    }
}

/// Internal fountain codec errors (Rust `ur::fountain::Error`).
internal enum FountainError: Error, Equatable, Sendable {
    case cborDecode(String)
    case cborEncode(String)
    case emptyMessage
    case emptyPart
    case invalidFragmentLen
    case inconsistentPart
    case expectedItem
    case invalidPadding
}

extension FountainError: CustomStringConvertible {
    var description: String {
        switch self {
        case .cborDecode(let message):
            return message
        case .cborEncode(let message):
            return message
        case .emptyMessage:
            return "expected non-empty message"
        case .emptyPart:
            return "expected non-empty part"
        case .invalidFragmentLen:
            return "expected positive maximum fragment length"
        case .inconsistentPart:
            return "part is inconsistent with previous ones"
        case .expectedItem:
            return "expected item"
        case .invalidPadding:
            return "invalid padding"
        }
    }
}

/// Errors thrown by the internal weighted sampler.
internal enum WeightedSamplerError: Error, Equatable, Sendable {
    case negativeProbability
    case nonPositiveTotal
}
