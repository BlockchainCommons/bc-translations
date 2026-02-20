import BCRand
import Foundation
import XCTest
@testable import BCCrypto

final class PublicKeyEncryptionTests: XCTestCase {
    func testX25519Keys() {
        var rng = makeFakeRandomNumberGenerator()
        let privateKey = x25519NewPrivateKeyUsing(&rng)
        XCTAssertEqual(
            privateKey,
            Data(
                testHex: "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
            )
        )

        let publicKey = x25519PublicKeyFromPrivateKey(privateKey)
        XCTAssertEqual(
            publicKey,
            Data(
                testHex: "f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025"
            )
        )

        XCTAssertEqual(
            deriveAgreementPrivateKey(Data("password".utf8)),
            Data(
                testHex: "7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f"
            )
        )

        XCTAssertEqual(
            deriveSigningPrivateKey(Data("password".utf8)),
            Data(
                testHex: "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"
            )
        )
    }

    func testKeyAgreement() {
        var rng = makeFakeRandomNumberGenerator()

        let alicePrivateKey = x25519NewPrivateKeyUsing(&rng)
        let alicePublicKey = x25519PublicKeyFromPrivateKey(alicePrivateKey)

        let bobPrivateKey = x25519NewPrivateKeyUsing(&rng)
        let bobPublicKey = x25519PublicKeyFromPrivateKey(bobPrivateKey)

        let aliceShared = x25519SharedKey(alicePrivateKey, bobPublicKey)
        let bobShared = x25519SharedKey(bobPrivateKey, alicePublicKey)

        XCTAssertEqual(aliceShared, bobShared)
        XCTAssertEqual(
            aliceShared,
            Data(
                testHex: "1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23"
            )
        )
    }
}
