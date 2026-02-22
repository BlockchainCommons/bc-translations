import BCCrypto
import DCBOR
import Foundation
import SSKR

public enum BCComponentsError: Swift.Error, Equatable, LocalizedError {
    case invalidSize(dataType: String, expected: Int, actual: Int)
    case invalidData(dataType: String, reason: String)
    case dataTooShort(dataType: String, minimum: Int, actual: Int)
    case crypto(String)
    case cbor(CBORError)
    case sskr(SSKRError)
    case uri(String)
    case compression(String)
    case postQuantum(String)
    case levelMismatch
    case ssh(String)
    case general(String)

    public var errorDescription: String? {
        switch self {
        case .invalidSize(let dataType, let expected, let actual):
            return "invalid \(dataType) size: expected \(expected), got \(actual)"
        case .invalidData(let dataType, let reason):
            return "invalid \(dataType): \(reason)"
        case .dataTooShort(let dataType, let minimum, let actual):
            return "data too short: \(dataType) expected at least \(minimum), got \(actual)"
        case .crypto(let message):
            return "cryptographic operation failed: \(message)"
        case .cbor(let error):
            return "CBOR error: \(error.localizedDescription)"
        case .sskr(let error):
            return "SSKR error: \(error.localizedDescription)"
        case .uri(let message):
            return "invalid URI: \(message)"
        case .compression(let message):
            return "compression error: \(message)"
        case .postQuantum(let message):
            return "post-quantum cryptography error: \(message)"
        case .levelMismatch:
            return "signature level does not match key level"
        case .ssh(let message):
            return "SSH operation failed: \(message)"
        case .general(let message):
            return message
        }
    }

}

public typealias Error = BCComponentsError
public typealias Result<T> = Swift.Result<T, BCComponentsError>

extension BCComponentsError {
    static func fromCryptoError(_ error: BCCryptoError) -> BCComponentsError {
        .crypto(error.localizedDescription)
    }

    static func fromCBORError(_ error: CBORError) -> BCComponentsError {
        .cbor(error)
    }
}
