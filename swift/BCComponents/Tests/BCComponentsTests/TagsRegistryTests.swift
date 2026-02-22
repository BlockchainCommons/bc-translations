import BCComponents
import XCTest

@MainActor
final class TagsRegistryTests: XCTestCase {
    func testDigestSummaryUsesBCComponentsSummarizer() {
        registerTags()

        let digest = Digest.fromImage(Data("hello".utf8))
        XCTAssertEqual(
            digest.cbor.summary,
            "Digest(\(digest.shortDescription))"
        )
    }

    func testSignatureSummaryUsesSchemeAwareText() throws {
        registerTags()

        let message = Data("Wolf McNally".utf8)
        let keyData = hexData(
            "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
        )

        let schnorrPrivate = SigningPrivateKey.newSchnorr(try ECPrivateKey(keyData))
        let schnorrSignature = try schnorrPrivate.sign(message)
        XCTAssertEqual(schnorrSignature.cbor.summary, "Signature")

        let ecdsaPrivate = SigningPrivateKey.newEcdsa(try ECPrivateKey(keyData))
        let ecdsaSignature = try ecdsaPrivate.sign(message)
        XCTAssertEqual(ecdsaSignature.cbor.summary, "Signature(ecdsa)")
    }
}
