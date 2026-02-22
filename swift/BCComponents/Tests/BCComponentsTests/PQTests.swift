import BCComponents
import BCRand
import XCTest

@MainActor
final class PQTests: XCTestCase {
    private let message = Data(
        "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".utf8
    )

    private func assertMLKEMRoundtrip(
        _ level: MLKEM,
        expectedPrivateKeySize: Int,
        expectedPublicKeySize: Int,
        expectedCiphertextSize: Int
    ) throws {
        let (privateKey, publicKey) = level.keypair()
        let (sharedSecretA, ciphertext) = publicKey.encapsulateNewSharedSecret()

        XCTAssertEqual(privateKey.size, expectedPrivateKeySize)
        XCTAssertEqual(publicKey.size, expectedPublicKeySize)
        XCTAssertEqual(ciphertext.size, expectedCiphertextSize)

        let sharedSecretB = try privateKey.decapsulateSharedSecret(ciphertext)
        XCTAssertEqual(sharedSecretA, sharedSecretB)
    }

    func testMLKEM512() throws {
        try assertMLKEMRoundtrip(
            .mlkem512,
            expectedPrivateKeySize: 1632,
            expectedPublicKeySize: 800,
            expectedCiphertextSize: 768
        )
    }

    func testMLKEM768() throws {
        try assertMLKEMRoundtrip(
            .mlkem768,
            expectedPrivateKeySize: 2400,
            expectedPublicKeySize: 1184,
            expectedCiphertextSize: 1088
        )
    }

    func testMLKEM1024() throws {
        try assertMLKEMRoundtrip(
            .mlkem1024,
            expectedPrivateKeySize: 3168,
            expectedPublicKeySize: 1568,
            expectedCiphertextSize: 1568
        )
    }

    private func assertMLDSASigning(_ level: MLDSA) throws {
        let (privateKey, publicKey) = level.keypair()
        let signature = privateKey.sign(message)

        XCTAssertTrue(try publicKey.verify(signature, message))
        XCTAssertFalse(try publicKey.verify(signature, message.dropLast()))

        let anotherSignature = privateKey.sign(message)
        XCTAssertNotEqual(signature, anotherSignature)
    }

    func testMLDSA44Signing() throws {
        try assertMLDSASigning(.mldsa44)
    }

    func testMLDSA65Signing() throws {
        try assertMLDSASigning(.mldsa65)
    }

    func testMLDSA87Signing() throws {
        try assertMLDSASigning(.mldsa87)
    }

    func testMLDSALevelCBORRoundtrip() throws {
        for level in [MLDSA.mldsa44, .mldsa65, .mldsa87] {
            let decoded = try MLDSA(cbor: level.cbor)
            XCTAssertEqual(level, decoded)
        }
    }

    func testMLDSAAndSigningIntegration() throws {
        let (mldsaPrivate, mldsaPublic) = MLDSA.mldsa65.keypair()
        let signingPrivate = SigningPrivateKey.newMLDSA(mldsaPrivate)
        let signingPublic = SigningPublicKey.fromMLDSA(mldsaPublic)

        XCTAssertThrowsError(try signingPrivate.publicKey())

        let signature = try signingPrivate.sign(message)
        guard case .mldsa(let mldsaSignature) = signature else {
            XCTFail("Expected MLDSA signature")
            return
        }
        XCTAssertEqual(mldsaSignature.level, .mldsa65)
        XCTAssertEqual(signature.scheme, .mldsa65)
        XCTAssertTrue(signingPublic.verify(signature, message))
    }

    func testMLDSAKeyAndSignatureCBORRoundtrip() throws {
        let (privateKey, publicKey) = MLDSA.mldsa44.keypair()
        let signature = privateKey.sign(message)

        let decodedPrivate = try MLDSAPrivateKey(cbor: privateKey.taggedCBOR)
        let decodedPublic = try MLDSAPublicKey(cbor: publicKey.taggedCBOR)
        let decodedSignature = try MLDSASignature(cbor: signature.taggedCBOR)

        XCTAssertEqual(privateKey, decodedPrivate)
        XCTAssertEqual(publicKey, decodedPublic)
        XCTAssertEqual(signature, decodedSignature)
    }

    func testMLDSALevelMismatchFails() throws {
        let (privateKey44, _) = MLDSA.mldsa44.keypair()
        let (_, publicKey65) = MLDSA.mldsa65.keypair()
        let signature44 = privateKey44.sign(message)

        XCTAssertThrowsError(try publicKey65.verify(signature44, message)) { error in
            guard let error = error as? BCComponentsError else {
                XCTFail("Unexpected error type: \(error)")
                return
            }
            XCTAssertEqual(error, .levelMismatch)
        }
    }

    func testSignatureSchemeMLDSAKeypairs() throws {
        for scheme in [SignatureScheme.mldsa44, .mldsa65, .mldsa87] {
            let (privateKey, publicKey) = scheme.keypair()
            let signature = try privateKey.sign(message)
            XCTAssertTrue(publicKey.verify(signature, message))
        }
    }

    func testSignatureSchemeMLDSAKeypairUsingFails() {
        var rng = makeFakeRandomNumberGenerator()
        XCTAssertThrowsError(try SignatureScheme.mldsa44.keypairUsing(&rng))
        XCTAssertThrowsError(try SignatureScheme.mldsa65.keypairUsing(&rng))
        XCTAssertThrowsError(try SignatureScheme.mldsa87.keypairUsing(&rng))
    }
}
