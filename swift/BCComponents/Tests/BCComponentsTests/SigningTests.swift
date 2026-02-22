import BCComponents
import BCRand
import XCTest

@MainActor
final class SigningTests: XCTestCase {
    private let message = Data("Wolf McNally".utf8)
    private let keyData = hexData("322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36")

    private var schnorrPrivateKey: SigningPrivateKey {
        .newSchnorr(try! ECPrivateKey(keyData))
    }

    private var ecdsaPrivateKey: SigningPrivateKey {
        .newEcdsa(try! ECPrivateKey(keyData))
    }

    private var ed25519PrivateKey: SigningPrivateKey {
        .newEd25519(try! Ed25519PrivateKey(keyData))
    }

    func testSchnorrSigning() throws {
        let publicKey = try schnorrPrivateKey.publicKey()
        let signature = try schnorrPrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try schnorrPrivateKey.sign(message)
        XCTAssertNotEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testSchnorrCBORVector() throws {
        registerTags()

        let rng = AnyBCRandomNumberGenerator(makeFakeRandomNumberGenerator())
        let signature = try schnorrPrivateKey.signWithOptions(
            message,
            options: .schnorr(rng: rng)
        )
        XCTAssertEqual(
            hexString(signature.toSchnorr()!),
            "9d113392074dd52dfb7f309afb3698a1993cd14d32bc27c00070407092c9ec8c096643b5b1b535bb5277c44f256441ac660cd600739aa910b150d4f94757cf95"
        )

        let cbor = signature.taggedCBOR
        XCTAssertEqual(
            hexString(cbor.cborData),
            "d99c5458409d113392074dd52dfb7f309afb3698a1993cd14d32bc27c00070407092c9ec8c096643b5b1b535bb5277c44f256441ac660cd600739aa910b150d4f94757cf95"
        )

        let decoded = try Signature(cbor: cbor)
        XCTAssertEqual(decoded, signature)
    }

    func testECDSASigning() throws {
        let publicKey = try ecdsaPrivateKey.publicKey()
        let signature = try ecdsaPrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try ecdsaPrivateKey.sign(message)
        XCTAssertEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testECDSACBORVector() throws {
        registerTags()

        let signature = try ecdsaPrivateKey.sign(message)
        XCTAssertEqual(
            hexString(signature.toEcdsa()!),
            "1458d0f3d97e25109b38fd965782b43213134d02b01388a14e74ebf21e5dea4866f25a23866de9ecf0f9b72404d8192ed71fba4dc355cd89b47213e855cf6d23"
        )

        let cbor = signature.taggedCBOR
        XCTAssertEqual(
            hexString(cbor.cborData),
            "d99c54820158401458d0f3d97e25109b38fd965782b43213134d02b01388a14e74ebf21e5dea4866f25a23866de9ecf0f9b72404d8192ed71fba4dc355cd89b47213e855cf6d23"
        )

        let decoded = try Signature(cbor: cbor)
        XCTAssertEqual(decoded, signature)
    }

    func testEd25519Signing() throws {
        let publicKey = try ed25519PrivateKey.publicKey()
        let signature = try ed25519PrivateKey.sign(message)

        XCTAssertTrue(publicKey.verify(signature, message))
        XCTAssertFalse(publicKey.verify(signature, Data("Wolf Mcnally".utf8)))

        let anotherSignature = try ed25519PrivateKey.sign(message)
        XCTAssertEqual(signature, anotherSignature)
        XCTAssertTrue(publicKey.verify(anotherSignature, message))
    }

    func testSchemeKeypairs() throws {
        for scheme in [SignatureScheme.default, .ecdsa, .ed25519] {
            let (privateKey, publicKey) = scheme.keypair()
            let signature = try privateKey.sign(message)
            XCTAssertTrue(publicKey.verify(signature, message))
        }
    }
}
