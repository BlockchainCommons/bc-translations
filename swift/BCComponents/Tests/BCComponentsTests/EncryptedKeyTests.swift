import BCComponents
import XCTest

@MainActor
final class EncryptedKeyTests: XCTestCase {
    private let secret = Data("correct horse battery staple".utf8)
    private let wrongSecret = Data("wrong secret".utf8)

    private func contentKey() -> SymmetricKey {
        SymmetricKey()
    }

    private func assertRoundtrip(
        method: KeyDerivationMethod,
        expectedDescription: String
    ) throws {
        let key = contentKey()
        let encrypted = try EncryptedKey.lock(
            method: method,
            secret: secret,
            contentKey: key
        )

        XCTAssertEqual(encrypted.description, expectedDescription)

        let decoded = try EncryptedKey(cbor: encrypted.taggedCBOR)
        let unlocked = try decoded.unlock(secret: secret)
        XCTAssertEqual(unlocked, key)
    }

    func testEncryptedKeyHKDFRoundtrip() throws {
        try assertRoundtrip(
            method: .hkdf,
            expectedDescription: "EncryptedKey(HKDF(SHA256))"
        )
    }

    func testEncryptedKeyPBKDF2Roundtrip() throws {
        try assertRoundtrip(
            method: .pbkdf2,
            expectedDescription: "EncryptedKey(PBKDF2(SHA256))"
        )
    }

    func testEncryptedKeyScryptRoundtrip() throws {
        try assertRoundtrip(
            method: .scrypt,
            expectedDescription: "EncryptedKey(Scrypt)"
        )
    }

    func testEncryptedKeyArgon2idRoundtrip() throws {
        try assertRoundtrip(
            method: .argon2id,
            expectedDescription: "EncryptedKey(Argon2id)"
        )
    }

    func testEncryptedKeyWrongSecretFails() throws {
        for method in [
            KeyDerivationMethod.hkdf,
            .pbkdf2,
            .scrypt,
            .argon2id,
        ] {
            let encrypted = try EncryptedKey.lock(
                method: method,
                secret: secret,
                contentKey: contentKey()
            )
            XCTAssertThrowsError(try encrypted.unlock(secret: wrongSecret))
        }
    }

    func testEncryptedKeyMethodDiscriminator() throws {
        for method in [
            KeyDerivationMethod.hkdf,
            .pbkdf2,
            .scrypt,
            .argon2id,
        ] {
            let encrypted = try EncryptedKey.lock(
                method: method,
                secret: secret,
                contentKey: contentKey()
            )
            let aad = try encrypted.aadCBOR
            guard case .array(let elements) = aad else {
                XCTFail("Expected AAD to decode to array")
                return
            }
            XCTAssertFalse(elements.isEmpty)
            guard case .unsigned(let methodIndex) = elements[0] else {
                XCTFail("Expected first AAD element to be an unsigned method index")
                return
            }
            XCTAssertEqual(methodIndex, UInt64(method.index()))
        }
    }
}
