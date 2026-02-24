import Foundation
import BCComponents

extension EnvelopeError {
    static let unknownSecret = EnvelopeError("unknownSecret")
}

public extension Envelope {
    func lockSubject(
        method: KeyDerivationMethod,
        secret: some DataProtocol
    ) throws -> Envelope {
        let contentKey = SymmetricKey()
        let encryptedKey = try EncryptedKey.lock(
            method: method,
            secret: secret,
            contentKey: contentKey
        )
        return try encryptSubject(with: contentKey)
            .addAssertion(.hasSecret, encryptedKey)
    }

    func unlockSubject(secret: some DataProtocol) throws -> Envelope {
        for assertion in assertions(withPredicate: .hasSecret) {
            guard let object = assertion.object, !object.isObscured else {
                continue
            }
            guard let encryptedKey = try? object.extractSubject(EncryptedKey.self) else {
                continue
            }
            if let contentKey = try? encryptedKey.unlock(secret: secret) {
                return try decryptSubject(with: contentKey)
            }
        }
        throw EnvelopeError.unknownSecret
    }

    func isLockedWithPassword() -> Bool {
        assertions(withPredicate: .hasSecret)
            .contains { assertion in
                guard
                    let object = assertion.object,
                    let encryptedKey = try? object.extractSubject(EncryptedKey.self)
                else {
                    return false
                }
                return encryptedKey.isPasswordBased
            }
    }

    func isLockedWithSSHAgent() -> Bool {
        assertions(withPredicate: .hasSecret)
            .contains { assertion in
                guard
                    let object = assertion.object,
                    let encryptedKey = try? object.extractSubject(EncryptedKey.self)
                else {
                    return false
                }
                return encryptedKey.isSSHAgent
            }
    }

    func addSecret(
        method: KeyDerivationMethod,
        secret: some DataProtocol,
        contentKey: SymmetricKey
    ) throws -> Envelope {
        let encryptedKey = try EncryptedKey.lock(
            method: method,
            secret: secret,
            contentKey: contentKey
        )
        return addAssertion(.hasSecret, encryptedKey)
    }

    func lock(
        method: KeyDerivationMethod,
        secret: some DataProtocol
    ) throws -> Envelope {
        try wrap().lockSubject(method: method, secret: secret)
    }

    func unlock(secret: some DataProtocol) throws -> Envelope {
        try unlockSubject(secret: secret).unwrap()
    }
}
