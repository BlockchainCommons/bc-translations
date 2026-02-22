import DCBOR

public enum KeyDerivationMethod: UInt64, CaseIterable, Equatable, Hashable, Sendable {
    case hkdf = 0
    case pbkdf2 = 1
    case scrypt = 2
    case argon2id = 3

    public static var `default`: KeyDerivationMethod {
        .argon2id
    }

    public func index() -> Int {
        Int(rawValue)
    }

    public static func fromIndex(_ index: Int) -> KeyDerivationMethod? {
        KeyDerivationMethod(rawValue: UInt64(index))
    }
}

extension KeyDerivationMethod: CustomStringConvertible {
    public var description: String {
        switch self {
        case .hkdf:
            return "HKDF"
        case .pbkdf2:
            return "PBKDF2"
        case .scrypt:
            return "Scrypt"
        case .argon2id:
            return "Argon2id"
        }
    }
}

extension KeyDerivationMethod: CBOREncodable {
    public var cbor: CBOR {
        .unsigned(rawValue)
    }
}

extension KeyDerivationMethod: CBORDecodable {
    public init(cbor: CBOR) throws {
        let index = try Int(cbor: cbor)
        guard let method = KeyDerivationMethod.fromIndex(index) else {
            throw BCComponentsError.general("Invalid KeyDerivationMethod")
        }
        self = method
    }
}
