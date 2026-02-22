import BCCrypto
import DCBOR
import Foundation

public struct ScryptParams: Equatable, Sendable {
    private let salt: Salt
    private let logN: UInt8
    private let r: UInt32
    private let p: UInt32

    private init(salt: Salt, logN: UInt8, r: UInt32, p: UInt32) {
        self.salt = salt
        self.logN = logN
        self.r = r
        self.p = p
    }

    public init() {
        self = ScryptParams.new()
    }

    public static func new() -> ScryptParams {
        ScryptParams.newOpt(try! Salt.newWithLen(saltLength), 15, 8, 1)
    }

    public static func newOpt(
        _ salt: Salt,
        _ logN: UInt8,
        _ r: UInt32,
        _ p: UInt32
    ) -> ScryptParams {
        ScryptParams(salt: salt, logN: logN, r: r, p: p)
    }

    public func saltValue() -> Salt {
        salt
    }

    public func logNValue() -> UInt8 {
        logN
    }

    public func rValue() -> UInt32 {
        r
    }

    public func pValue() -> UInt32 {
        p
    }
}

extension ScryptParams: KeyDerivation {
    public static let index = KeyDerivationMethod.scrypt.index()

    public mutating func lock(
        _ contentKey: SymmetricKey,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> EncryptedMessage {
        let derivedData = scrypt(
            password: Data(secret),
            salt: salt.data,
            outputLength: SymmetricKey.symmetricKeySize,
            logN: logN,
            r: r,
            p: p
        )
        let derivedKey = try SymmetricKey(derivedData)
        return derivedKey.encrypt(contentKey.data, aad: cborData, nonce: nil)
    }

    public func unlock(
        _ encryptedMessage: EncryptedMessage,
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey {
        let derivedData = scrypt(
            password: Data(secret),
            salt: salt.data,
            outputLength: SymmetricKey.symmetricKeySize,
            logN: logN,
            r: r,
            p: p
        )
        let derivedKey = try SymmetricKey(derivedData)
        let contentKeyData = try derivedKey.decrypt(encryptedMessage)
        return try SymmetricKey(contentKeyData)
    }
}

extension ScryptParams: CustomStringConvertible {
    public var description: String {
        "Scrypt"
    }
}

extension ScryptParams: CBOREncodable {
    public var cbor: CBOR {
        .array([
            .unsigned(UInt64(Self.index)),
            salt.cbor,
            .unsigned(UInt64(logN)),
            .unsigned(UInt64(r)),
            .unsigned(UInt64(p)),
        ])
    }
}

extension ScryptParams: CBORDecodable {
    public init(cbor: CBOR) throws {
        guard case .array(let elements) = cbor, elements.count == 5 else {
            throw BCComponentsError.invalidData(
                dataType: "ScryptParams",
                reason: "invalid ScryptParams"
            )
        }
        _ = try Int(cbor: elements[0])
        let salt = try Salt(cbor: elements[1])
        let logN = try UInt8(cbor: elements[2])
        let r = try UInt32(cbor: elements[3])
        let p = try UInt32(cbor: elements[4])
        self.init(salt: salt, logN: logN, r: r, p: p)
    }
}
