import BCCrypto
import DCBOR
import Foundation

public struct PBKDF2Params: Equatable, Sendable {
    private let salt: Salt
    private let iterations: UInt32
    private let hashType: HashType

    private init(salt: Salt, iterations: UInt32, hashType: HashType) {
        self.salt = salt
        self.iterations = iterations
        self.hashType = hashType
    }

    public init() {
        self = PBKDF2Params.new()
    }

    public static func new() -> PBKDF2Params {
        PBKDF2Params.newOpt(try! Salt.newWithLen(saltLength), 100_000, .sha256)
    }

    public static func newOpt(
        _ salt: Salt,
        _ iterations: UInt32,
        _ hashType: HashType
    ) -> PBKDF2Params {
        PBKDF2Params(salt: salt, iterations: iterations, hashType: hashType)
    }

    public func saltValue() -> Salt {
        salt
    }

    public func iterationsValue() -> UInt32 {
        iterations
    }

    public func hashTypeValue() -> HashType {
        hashType
    }
}

extension PBKDF2Params: KeyDerivation {
    public static let index = KeyDerivationMethod.pbkdf2.index()

    public mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage {
        let secretData = Data(secret)
        let saltData = salt.data
        let derivedData: Data
        switch hashType {
        case .sha256:
            derivedData = pbkdf2HmacSHA256(
                password: secretData,
                salt: saltData,
                iterations: iterations,
                keyLength: SymmetricKey.symmetricKeySize
            )
        case .sha512:
            derivedData = pbkdf2HmacSHA512(
                password: secretData,
                salt: saltData,
                iterations: iterations,
                keyLength: SymmetricKey.symmetricKeySize
            )
        }
        let derivedKey = try SymmetricKey(derivedData)
        return derivedKey.encrypt(contentKey.data, aad: cborData, nonce: nil)
    }

    public func unlock(
        _ encryptedMessage: EncryptedMessage,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey {
        let secretData = Data(secret)
        let saltData = salt.data
        let derivedData: Data
        switch hashType {
        case .sha256:
            derivedData = pbkdf2HmacSHA256(
                password: secretData,
                salt: saltData,
                iterations: iterations,
                keyLength: SymmetricKey.symmetricKeySize
            )
        case .sha512:
            derivedData = pbkdf2HmacSHA512(
                password: secretData,
                salt: saltData,
                iterations: iterations,
                keyLength: SymmetricKey.symmetricKeySize
            )
        }
        let derivedKey = try SymmetricKey(derivedData)
        let contentKeyData = try derivedKey.decrypt(encryptedMessage)
        return try SymmetricKey(contentKeyData)
    }
}

extension PBKDF2Params: CustomStringConvertible {
    public var description: String {
        "PBKDF2(\(hashType))"
    }
}

extension PBKDF2Params: CBOREncodable {
    public var cbor: CBOR {
        .array([
            .unsigned(UInt64(Self.index)),
            salt.cbor,
            .unsigned(UInt64(iterations)),
            hashType.cbor,
        ])
    }
}

extension PBKDF2Params: CBORDecodable {
    public init(cbor: CBOR) throws {
        guard case .array(let elements) = cbor, elements.count == 4 else {
            throw BCComponentsError.invalidData(
                dataType: "PBKDF2Params",
                reason: "invalid PBKDF2Params"
            )
        }
        _ = try Int(cbor: elements[0])
        let salt = try Salt(cbor: elements[1])
        let iterations = try UInt32(cbor: elements[2])
        let hashType = try HashType(cbor: elements[3])
        self.init(salt: salt, iterations: iterations, hashType: hashType)
    }
}
