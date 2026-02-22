import DCBOR
import Foundation

public enum KeyDerivationParams: Equatable, Sendable {
    case hkdf(HKDFParams)
    case pbkdf2(PBKDF2Params)
    case scrypt(ScryptParams)
    case argon2id(Argon2idParams)

    public func method() -> KeyDerivationMethod {
        switch self {
        case .hkdf:
            return .hkdf
        case .pbkdf2:
            return .pbkdf2
        case .scrypt:
            return .scrypt
        case .argon2id:
            return .argon2id
        }
    }

    public func isPasswordBased() -> Bool {
        switch self {
        case .hkdf:
            return false
        case .pbkdf2, .scrypt, .argon2id:
            return true
        }
    }

    public func isSSHAgent() -> Bool {
        false
    }

    public mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage {
        switch self {
        case .hkdf(var params):
            return try params.lock(contentKey, secret: secret)
        case .pbkdf2(var params):
            return try params.lock(contentKey, secret: secret)
        case .scrypt(var params):
            return try params.lock(contentKey, secret: secret)
        case .argon2id(var params):
            return try params.lock(contentKey, secret: secret)
        }
    }
}

extension KeyDerivationParams: CustomStringConvertible {
    public var description: String {
        switch self {
        case .hkdf(let params):
            return params.description
        case .pbkdf2(let params):
            return params.description
        case .scrypt(let params):
            return params.description
        case .argon2id(let params):
            return params.description
        }
    }
}

extension KeyDerivationParams: CBOREncodable {
    public var cbor: CBOR {
        switch self {
        case .hkdf(let params):
            return params.cbor
        case .pbkdf2(let params):
            return params.cbor
        case .scrypt(let params):
            return params.cbor
        case .argon2id(let params):
            return params.cbor
        }
    }
}

extension KeyDerivationParams: CBORDecodable {
    public init(cbor: CBOR) throws {
        guard case .array(let elements) = cbor,
              let first = elements.first
        else {
            throw BCComponentsError.invalidData(
                dataType: "KeyDerivationParams",
                reason: "missing method"
            )
        }
        let index = try Int(cbor: first)
        guard let method = KeyDerivationMethod.fromIndex(index) else {
            throw BCComponentsError.general("Invalid KeyDerivationMethod")
        }
        switch method {
        case .hkdf:
            self = .hkdf(try HKDFParams(cbor: cbor))
        case .pbkdf2:
            self = .pbkdf2(try PBKDF2Params(cbor: cbor))
        case .scrypt:
            self = .scrypt(try ScryptParams(cbor: cbor))
        case .argon2id:
            self = .argon2id(try Argon2idParams(cbor: cbor))
        }
    }
}
