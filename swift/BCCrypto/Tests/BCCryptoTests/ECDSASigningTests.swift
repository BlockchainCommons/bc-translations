import BCRand
import Foundation
import XCTest
@testable import BCCrypto

final class ECDSASigningTests: XCTestCase {
    func testECDSASigning() {
        var rng = makeFakeRandomNumberGenerator()

        let privateKey = ecdsaNewPrivateKeyUsing(&rng)
        let publicKey = ecdsaPublicKeyFromPrivateKey(privateKey)
        let signature = ecdsaSign(privateKey, longMessage)

        XCTAssertEqual(
            signature,
            Data(
                testHex: "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
            )
        )
        XCTAssertTrue(ecdsaVerify(publicKey, signature, longMessage))
    }
}
