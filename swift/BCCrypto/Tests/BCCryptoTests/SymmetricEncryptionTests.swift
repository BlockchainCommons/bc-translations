import BCRand
import Foundation
import XCTest
@testable import BCCrypto

final class SymmetricEncryptionTests: XCTestCase {
    private let plaintext = longMessage
    private let aad = Data(testHex: "50515253c0c1c2c3c4c5c6c7")
    private let key = Data(
        testHex: "808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f"
    )
    private let nonce = Data(testHex: "070000004041424344454647")
    private let ciphertext = Data(
        testHex: "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116"
    )
    private let auth = Data(testHex: "1ae10b594f09e26a7e902ecbd0600691")

    func testRFCTestVector() throws {
        let (actualCiphertext, actualAuth) = aeadChaCha20Poly1305EncryptWithAAD(
            plaintext,
            key,
            nonce,
            aad
        )
        XCTAssertEqual(actualCiphertext, ciphertext)
        XCTAssertEqual(actualAuth, auth)

        let decrypted = try aeadChaCha20Poly1305DecryptWithAAD(
            actualCiphertext,
            key,
            nonce,
            aad,
            actualAuth
        )
        XCTAssertEqual(decrypted, plaintext)
    }

    func testRandomKeyAndNonce() throws {
        let key = randomData(count: 32)
        let nonce = randomData(count: 12)

        let (ciphertext, auth) = aeadChaCha20Poly1305EncryptWithAAD(
            plaintext,
            key,
            nonce,
            aad
        )
        let decrypted = try aeadChaCha20Poly1305DecryptWithAAD(
            ciphertext,
            key,
            nonce,
            aad,
            auth
        )
        XCTAssertEqual(decrypted, plaintext)
    }

    func testEmptyData() throws {
        let key = randomData(count: 32)
        let nonce = randomData(count: 12)

        let (ciphertext, auth) = aeadChaCha20Poly1305EncryptWithAAD(
            Data(),
            key,
            nonce,
            Data()
        )
        let decrypted = try aeadChaCha20Poly1305DecryptWithAAD(
            ciphertext,
            key,
            nonce,
            Data(),
            auth
        )
        XCTAssertEqual(decrypted, Data())
    }
}
