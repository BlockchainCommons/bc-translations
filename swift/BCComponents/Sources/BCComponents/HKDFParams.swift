import BCCrypto
import DCBOR
import Foundation

public struct HKDFParams: Equatable, Sendable {
    private let salt: Salt
    private let hashType: HashType

    private init(salt: Salt, hashType: HashType) {
        self.salt = salt
        self.hashType = hashType
    }

    public init() {
        self = HKDFParams.new()
    }

    public static func new() -> HKDFParams {
        HKDFParams.newOpt(try! Salt.newWithLen(SALT_LEN), .sha256)
    }

    public static func newOpt(_ salt: Salt, _ hashType: HashType) -> HKDFParams {
        HKDFParams(salt: salt, hashType: hashType)
    }

    public func saltValue() -> Salt {
        salt
    }

    public func hashTypeValue() -> HashType {
        hashType
    }
}

extension HKDFParams: KeyDerivation {
    public static let index = KeyDerivationMethod.hkdf.index()

    public mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage {
        let secretData = Data(secret)
        let saltData = salt.asBytes()
        let derivedData: Data
        switch hashType {
        case .sha256:
            derivedData = hkdfHmacSHA256(
                keyMaterial: secretData,
                salt: saltData,
                keyLength: SymmetricKey.symmetricKeySize
            )
        case .sha512:
            derivedData = hkdfHmacSHA512(
                keyMaterial: secretData,
                salt: saltData,
                keyLength: SymmetricKey.symmetricKeySize
            )
        }
        let derivedKey = try SymmetricKey.fromData(derivedData)
        return derivedKey.encrypt(contentKey.asBytes(), aad: cborData, nonce: nil)
    }

    public func unlock(
        _ encryptedMessage: EncryptedMessage,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey {
        let secretData = Data(secret)
        let saltData = salt.asBytes()
        let derivedData: Data
        switch hashType {
        case .sha256:
            derivedData = hkdfHmacSHA256(
                keyMaterial: secretData,
                salt: saltData,
                keyLength: SymmetricKey.symmetricKeySize
            )
        case .sha512:
            derivedData = hkdfHmacSHA512(
                keyMaterial: secretData,
                salt: saltData,
                keyLength: SymmetricKey.symmetricKeySize
            )
        }
        let derivedKey = try SymmetricKey.fromData(derivedData)
        let contentKeyData = try derivedKey.decrypt(encryptedMessage)
        return try SymmetricKey.fromData(contentKeyData)
    }
}

extension HKDFParams: CustomStringConvertible {
    public var description: String {
        "HKDF(\(hashType))"
    }
}

extension HKDFParams: CBOREncodable {
    public var cbor: CBOR {
        .array([
            .unsigned(UInt64(Self.index)),
            salt.cbor,
            hashType.cbor,
        ])
    }
}

extension HKDFParams: CBORDecodable {
    public init(cbor: CBOR) throws {
        guard case .array(let elements) = cbor, elements.count == 3 else {
            throw BCComponentsError.invalidData(
                dataType: "HKDFParams",
                reason: "invalid HKDFParams"
            )
        }
        _ = try Int(cbor: elements[0])
        let salt = try Salt(cbor: elements[1])
        let hashType = try HashType(cbor: elements[2])
        self.init(salt: salt, hashType: hashType)
    }
}
