import Foundation
import DCBOR

/// Errors that can occur during provenance mark operations.
public enum ProvenanceMarkError: Error, Sendable {
    /// The seed data is not exactly 32 bytes.
    case invalidSeedLength(actual: Int)

    /// A duplicate key was encountered during parsing.
    case duplicateKey(String)

    /// A required key is missing.
    case missingKey(String)

    /// A key has an invalid value.
    case invalidKey(String)

    /// The number of keys does not match the expected count.
    case extraKeys(expected: Int, actual: Int)

    /// The key field has the wrong byte length for the resolution.
    case invalidKeyLength(expected: Int, actual: Int)

    /// The next-key field has the wrong byte length for the resolution.
    case invalidNextKeyLength(expected: Int, actual: Int)

    /// The chain ID field has the wrong byte length for the resolution.
    case invalidChainIdLength(expected: Int, actual: Int)

    /// The message is shorter than the minimum length for the resolution.
    case invalidMessageLength(expected: Int, actual: Int)

    /// The info field contains invalid CBOR data.
    case invalidInfoCbor

    /// A date value is out of the representable range for serialization.
    case dateOutOfRange(details: String)

    /// A date has invalid components.
    case invalidDate(details: String)

    /// A required URL query parameter is missing.
    case missingUrlParameter(parameter: String)

    /// The year is outside the valid range for 2-byte date serialization.
    case yearOutOfRange(year: Int)

    /// The month or day is invalid for the given year.
    case invalidMonthOrDay(year: Int, month: Int, day: Int)

    /// An error in resolution-level serialization or deserialization.
    case resolutionError(details: String)

    /// A Bytewords or UR encoding/decoding error.
    case bytewords(String)

    /// A CBOR encoding/decoding error.
    case cbor(String)

    /// A URL parsing error.
    case url(String)

    /// A Base64 decoding error.
    case base64(String)

    /// A JSON serialization or deserialization error.
    case json(String)

    /// An integer conversion error.
    case integerConversion(String)

    /// An envelope encoding/decoding error.
    case envelope(String)

    /// A validation issue detected during chain verification.
    case validation(ValidationIssue)
}

// MARK: - LocalizedError

extension ProvenanceMarkError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .invalidSeedLength(let actual):
            return "invalid seed length: expected 32 bytes, got \(actual) bytes"
        case .duplicateKey(let key):
            return "duplicate key: \(key)"
        case .missingKey(let key):
            return "missing key: \(key)"
        case .invalidKey(let key):
            return "invalid key: \(key)"
        case .extraKeys(let expected, let actual):
            return "wrong number of keys: expected \(expected), got \(actual)"
        case .invalidKeyLength(let expected, let actual):
            return "invalid key length: expected \(expected), got \(actual)"
        case .invalidNextKeyLength(let expected, let actual):
            return "invalid next key length: expected \(expected), got \(actual)"
        case .invalidChainIdLength(let expected, let actual):
            return "invalid chain ID length: expected \(expected), got \(actual)"
        case .invalidMessageLength(let expected, let actual):
            return "invalid message length: expected at least \(expected), got \(actual)"
        case .invalidInfoCbor:
            return "invalid CBOR data in info field"
        case .dateOutOfRange(let details):
            return "date out of range: \(details)"
        case .invalidDate(let details):
            return "invalid date: \(details)"
        case .missingUrlParameter(let parameter):
            return "missing required URL parameter: \(parameter)"
        case .yearOutOfRange(let year):
            return "year out of range for 2-byte serialization: must be between 2023-2150, got \(year)"
        case .invalidMonthOrDay(let year, let month, let day):
            return "invalid month (\(month)) or day (\(day)) for year \(year)"
        case .resolutionError(let details):
            return "resolution serialization error: \(details)"
        case .bytewords(let message):
            return "bytewords error: \(message)"
        case .cbor(let message):
            return "CBOR error: \(message)"
        case .url(let message):
            return "URL parsing error: \(message)"
        case .base64(let message):
            return "base64 decoding error: \(message)"
        case .json(let message):
            return "JSON error: \(message)"
        case .integerConversion(let message):
            return "integer conversion error: \(message)"
        case .envelope(let message):
            return "envelope error: \(message)"
        case .validation(let issue):
            return "validation error: \(issue)"
        }
    }
}

// MARK: - CustomStringConvertible

extension ProvenanceMarkError: CustomStringConvertible {
    public var description: String {
        errorDescription ?? "unknown provenance mark error"
    }
}
