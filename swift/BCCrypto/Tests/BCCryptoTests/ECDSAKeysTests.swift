import BCRand
import Foundation
import XCTest
@testable import BCCrypto

final class ECDSAKeysTests: XCTestCase {
    func testECDSAKeys() {
        var rng = makeFakeRandomNumberGenerator()

        let privateKey = ecdsaNewPrivateKeyUsing(&rng)
        XCTAssertEqual(
            privateKey,
            Data(
                testHex: "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
            )
        )

        let publicKey = ecdsaPublicKeyFromPrivateKey(privateKey)
        XCTAssertEqual(
            publicKey,
            Data(
                testHex: "0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"
            )
        )

        let decompressed = ecdsaDecompressPublicKey(publicKey)
        XCTAssertEqual(
            decompressed,
            Data(
                testHex: "0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a"
            )
        )

        let compressed = ecdsaCompressPublicKey(decompressed)
        XCTAssertEqual(compressed, publicKey)

        let xOnly = schnorrPublicKeyFromPrivateKey(privateKey)
        XCTAssertEqual(
            xOnly,
            Data(
                testHex: "71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"
            )
        )

        let derived = ecdsaDerivePrivateKey(Data("password".utf8))
        XCTAssertEqual(
            derived,
            Data(
                testHex: "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"
            )
        )
    }
}
