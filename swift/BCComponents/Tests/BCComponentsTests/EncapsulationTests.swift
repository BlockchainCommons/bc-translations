import BCComponents
import BCRand
import XCTest

@MainActor
final class EncapsulationTests: XCTestCase {
    private func assertEncapsulationRoundtrip(
        _ scheme: EncapsulationScheme
    ) throws {
        let (privateKey, publicKey) = scheme.keypair()
        let (secretA, ciphertext) = publicKey.encapsulateNewSharedSecret()
        let secretB = try privateKey.decapsulateSharedSecret(ciphertext)
        XCTAssertEqual(secretA, secretB)
    }

    func testX25519Encapsulation() throws {
        try assertEncapsulationRoundtrip(.x25519)
    }

    func testMLKEM512Encapsulation() throws {
        try assertEncapsulationRoundtrip(.mlkem512)
    }

    func testMLKEM768Encapsulation() throws {
        try assertEncapsulationRoundtrip(.mlkem768)
    }

    func testMLKEM1024Encapsulation() throws {
        try assertEncapsulationRoundtrip(.mlkem1024)
    }

    func testSealedMessageX25519() throws {
        let plaintext = Data("Some mysteries aren't meant to be solved.".utf8)

        let (alicePrivateKey, _) = EncapsulationScheme.x25519.keypair()
        let (bobPrivateKey, bobPublicKey) = EncapsulationScheme.x25519.keypair()
        let (carolPrivateKey, _) = EncapsulationScheme.x25519.keypair()

        let sealedMessage = SealedMessage.new(plaintext, recipient: bobPublicKey)

        XCTAssertEqual(try sealedMessage.decrypt(bobPrivateKey), plaintext)
        XCTAssertThrowsError(try sealedMessage.decrypt(alicePrivateKey))
        XCTAssertThrowsError(try sealedMessage.decrypt(carolPrivateKey))
    }

    func testSealedMessageMLKEM512() throws {
        let plaintext = Data("Some mysteries aren't meant to be solved.".utf8)

        let (alicePrivateKey, _) = EncapsulationScheme.mlkem512.keypair()
        let (bobPrivateKey, bobPublicKey) = EncapsulationScheme.mlkem512.keypair()
        let (carolPrivateKey, _) = EncapsulationScheme.mlkem512.keypair()

        let sealedMessage = SealedMessage.new(plaintext, recipient: bobPublicKey)

        XCTAssertEqual(try sealedMessage.decrypt(bobPrivateKey), plaintext)
        XCTAssertThrowsError(try sealedMessage.decrypt(alicePrivateKey))
        XCTAssertThrowsError(try sealedMessage.decrypt(carolPrivateKey))
    }

    func testEncapsulationSchemeKeypairUsingMLKEMFails() {
        var rng = makeFakeRandomNumberGenerator()
        XCTAssertThrowsError(try EncapsulationScheme.mlkem512.keypairUsing(&rng))
        XCTAssertThrowsError(try EncapsulationScheme.mlkem768.keypairUsing(&rng))
        XCTAssertThrowsError(try EncapsulationScheme.mlkem1024.keypairUsing(&rng))
    }
}
