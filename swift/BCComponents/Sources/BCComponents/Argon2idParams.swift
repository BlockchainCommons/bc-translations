import BCCrypto
import DCBOR
import Foundation

public struct Argon2idParams: Equatable, Sendable {
    private let salt: Salt

    private init(salt: Salt) {
        self.salt = salt
    }

    public init() {
        self = Argon2idParams.new()
    }

    public static func new() -> Argon2idParams {
        Argon2idParams.newOpt(try! Salt.newWithLen(saltLength))
    }

    public static func newOpt(_ salt: Salt) -> Argon2idParams {
        Argon2idParams(salt: salt)
    }

    public func saltValue() -> Salt {
        salt
    }
}

extension Argon2idParams: KeyDerivation {
    public static let index = KeyDerivationMethod.argon2id.index()

    public mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage {
        let derivedData = argon2id(
            password: Data(secret),
            salt: salt.data,
            outputLength: SymmetricKey.symmetricKeySize
        )
        let derivedKey = try SymmetricKey(derivedData)
        return derivedKey.encrypt(contentKey.data, aad: cborData, nonce: nil)
    }

    public func unlock(
        _ encryptedMessage: EncryptedMessage,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey {
        let derivedData = argon2id(
            password: Data(secret),
            salt: salt.data,
            outputLength: SymmetricKey.symmetricKeySize
        )
        let derivedKey = try SymmetricKey(derivedData)
        let contentKeyData = try derivedKey.decrypt(encryptedMessage)
        return try SymmetricKey(contentKeyData)
    }
}

extension Argon2idParams: CustomStringConvertible {
    public var description: String {
        "Argon2id"
    }
}

extension Argon2idParams: CBOREncodable {
    public var cbor: CBOR {
        .array([
            .unsigned(UInt64(Self.index)),
            salt.cbor,
        ])
    }
}

extension Argon2idParams: CBORDecodable {
    public init(cbor: CBOR) throws {
        guard case .array(let elements) = cbor, elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "Argon2idParams",
                reason: "invalid Argon2idParams"
            )
        }
        _ = try Int(cbor: elements[0])
        let salt = try Salt(cbor: elements[1])
        self.init(salt: salt)
    }
}
