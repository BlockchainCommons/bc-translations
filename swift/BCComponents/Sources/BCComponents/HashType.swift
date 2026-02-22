import DCBOR

public enum HashType: UInt64, CaseIterable, Equatable, Hashable, Sendable {
    case sha256 = 0
    case sha512 = 1
}

extension HashType: CustomStringConvertible {
    public var description: String {
        switch self {
        case .sha256:
            return "SHA256"
        case .sha512:
            return "SHA512"
        }
    }
}

extension HashType: CBOREncodable {
    public var cbor: CBOR {
        .unsigned(rawValue)
    }
}

extension HashType: CBORDecodable {
    public init(cbor: CBOR) throws {
        let value = try UInt64(cbor: cbor)
        guard let hashType = HashType(rawValue: value) else {
            throw BCComponentsError.general("Invalid HashType")
        }
        self = hashType
    }
}
