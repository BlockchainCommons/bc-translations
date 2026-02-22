import BCUR
import BCTags
import DCBOR
import Foundation

public let SALT_LEN = 16

public struct EncryptedKey: Equatable, Sendable {
    private let params: KeyDerivationParams
    private let encryptedMessage: EncryptedMessage

    public init(params: KeyDerivationParams, encryptedMessage: EncryptedMessage) {
        self.params = params
        self.encryptedMessage = encryptedMessage
    }

    public static func lockOpt(
        params: KeyDerivationParams,
        secret: some DataProtocol,
        contentKey: SymmetricKey
    ) throws(BCComponentsError) -> EncryptedKey {
        var params = params
        let encryptedMessage = try params.lock(contentKey, secret: secret)
        return EncryptedKey(params: params, encryptedMessage: encryptedMessage)
    }

    public static func lock(
        method: KeyDerivationMethod,
        secret: some DataProtocol,
        contentKey: SymmetricKey
    ) throws(BCComponentsError) -> EncryptedKey {
        switch method {
        case .hkdf:
            return try lockOpt(
                params: .hkdf(.new()),
                secret: secret,
                contentKey: contentKey
            )
        case .pbkdf2:
            return try lockOpt(
                params: .pbkdf2(.new()),
                secret: secret,
                contentKey: contentKey
            )
        case .scrypt:
            return try lockOpt(
                params: .scrypt(.new()),
                secret: secret,
                contentKey: contentKey
            )
        case .argon2id:
            return try lockOpt(
                params: .argon2id(.new()),
                secret: secret,
                contentKey: contentKey
            )
        }
    }

    public func encryptedMessageValue() -> EncryptedMessage {
        encryptedMessage
    }

    public func aadCBOR() throws(BCComponentsError) -> CBOR {
        guard let cbor = encryptedMessage.aadCBOR() else {
            throw BCComponentsError.general("Missing AAD CBOR in EncryptedMessage")
        }
        return cbor
    }

    public func unlock(
        secret: some DataProtocol
    ) throws(BCComponentsError) -> SymmetricKey {
        let cbor = try aadCBOR()
        guard case .array(let array) = cbor,
              let first = array.first
        else {
            throw BCComponentsError.general("Missing method")
        }

        let method: KeyDerivationMethod
        do {
            method = try KeyDerivationMethod(cbor: first)
        } catch {
            throw encryptedKeyError(error as any Swift.Error)
        }

        switch method {
        case .hkdf:
            do {
                return try HKDFParams(cbor: cbor).unlock(
                    encryptedMessage,
                    secret: secret
                )
            } catch {
                throw encryptedKeyError(error as any Swift.Error)
            }
        case .pbkdf2:
            do {
                return try PBKDF2Params(cbor: cbor).unlock(
                    encryptedMessage,
                    secret: secret
                )
            } catch {
                throw encryptedKeyError(error as any Swift.Error)
            }
        case .scrypt:
            do {
                return try ScryptParams(cbor: cbor).unlock(
                    encryptedMessage,
                    secret: secret
                )
            } catch {
                throw encryptedKeyError(error as any Swift.Error)
            }
        case .argon2id:
            do {
                return try Argon2idParams(cbor: cbor).unlock(
                    encryptedMessage,
                    secret: secret
                )
            } catch {
                throw encryptedKeyError(error as any Swift.Error)
            }
        }
    }

    public func isPasswordBased() -> Bool {
        params.isPasswordBased()
    }

    public func isSSHAgent() -> Bool {
        params.isSSHAgent()
    }
}

extension EncryptedKey: CustomStringConvertible {
    public var description: String {
        "EncryptedKey(\(params))"
    }
}

extension EncryptedKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.encryptedKey]
    }

    public var untaggedCBOR: CBOR {
        encryptedMessage.cbor
    }
}

extension EncryptedKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        let encryptedMessage = try EncryptedMessage(cbor: untaggedCBOR)
        let aadData = encryptedMessage.aad()
        let paramsCBOR = try CBOR(aadData)
        let params = try KeyDerivationParams(cbor: paramsCBOR)
        self.init(params: params, encryptedMessage: encryptedMessage)
    }
}

extension EncryptedKey: URCodable {}

private func encryptedKeyError(_ error: any Swift.Error) -> BCComponentsError {
    if let error = error as? BCComponentsError {
        return error
    }
    if let error = error as? CBORError {
        return .cbor(error)
    }
    return .general(error.localizedDescription)
}
