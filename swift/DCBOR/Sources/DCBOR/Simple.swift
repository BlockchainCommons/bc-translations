import Foundation

/// A CBOR simple value.
public enum Simple: Equatable, Sendable {
    /// The boolean value `false`.
    case `false`
    /// The boolean value `true`.
    case `true`
    /// The value representing `null` (`nil`).
    case null
    /// A floating point value.
    case float(Double)
}

extension Simple: CBORCodable {
    public var cbor: CBOR {
        .simple(self)
    }

    public var cborData: Data {
        switch self {
        case .false:
            return 20.encodeVarInt(.simple)
        case .true:
            return 21.encodeVarInt(.simple)
        case .null:
            return 22.encodeVarInt(.simple)
        case .float(let n):
            return n.cborData
        }
    }

    public init(cbor: CBOR) throws {
        switch cbor {
        case .simple(let s):
            self = s
        default:
            throw CBORError.wrongType
        }
    }
}

extension Simple: CustomDebugStringConvertible {
    public var debugDescription: String {
        switch self {
        case .false:
            return "false"
        case .true:
            return "true"
        case .null:
            return "null"
        case .float(let n):
            return String(n)
        }
    }
}

extension Simple: CustomStringConvertible {
    public var description: String {
        switch self {
        case .false:
            return "false"
        case .true:
            return "true"
        case .null:
            return "null"
        case .float(let n):
            return String(n)
        }
    }
}
