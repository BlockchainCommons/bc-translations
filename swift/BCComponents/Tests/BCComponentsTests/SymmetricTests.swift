import BCComponents
import BCTags
import DCBOR
import XCTest

@MainActor
final class SymmetricTests: XCTestCase {
    private let plaintext = Data("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".utf8)
    private let aad = hexData("50515253c0c1c2c3c4c5c6c7")
    private let key = try! SymmetricKey(hexData("808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f"))
    private let nonce = try! Nonce(hexData("070000004041424344454647"))
    private let expectedCiphertext = hexData("d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116")
    private let expectedAuth = try! AuthenticationTag(hexData("1ae10b594f09e26a7e902ecbd0600691"))

    func testRFCVector() throws {
        let encrypted = key.encrypt(plaintext, aad: aad, nonce: nonce)
        XCTAssertEqual(encrypted.ciphertext, expectedCiphertext)
        XCTAssertEqual(encrypted.aad, aad)
        XCTAssertEqual(encrypted.nonce, nonce)
        XCTAssertEqual(encrypted.authenticationTag, expectedAuth)

        let decrypted = try key.decrypt(encrypted)
        XCTAssertEqual(decrypted, plaintext)
    }

    func testRandomKeyAndNonce() throws {
        let key = SymmetricKey()
        let nonce = Nonce()
        let encrypted = key.encrypt(plaintext, aad: aad, nonce: nonce)
        let decrypted = try key.decrypt(encrypted)
        XCTAssertEqual(decrypted, plaintext)
    }

    func testEmptyData() throws {
        let key = SymmetricKey()
        let encrypted = key.encrypt(
            Data(),
            aad: Optional<Data>.none,
            nonce: Optional<Nonce>.none
        )
        let decrypted = try key.decrypt(encrypted)
        XCTAssertEqual(decrypted, Data())
    }

    func testCBORDataVector() throws {
        BCTags.registerTags()

        let encrypted = key.encrypt(plaintext, aad: aad, nonce: nonce)
        let cbor = encrypted.cbor
        XCTAssertEqual(
            hexString(cbor.cborData),
            "d99c42845872d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b61164c070000004041424344454647501ae10b594f09e26a7e902ecbd06006914c50515253c0c1c2c3c4c5c6c7"
        )
    }

    func testCBORRoundtrip() throws {
        let encrypted = key.encrypt(plaintext, aad: aad, nonce: nonce)
        let decoded = try EncryptedMessage(cbor: encrypted.cbor)
        XCTAssertEqual(decoded, encrypted)
    }

    func testURRoundtrip() throws {
        BCTags.registerTags()

        let encrypted = key.encrypt(plaintext, aad: aad, nonce: nonce)
        let ur = encrypted.urString()
        XCTAssertEqual(
            ur,
            "ur:encrypted/lrhdjptecylgeeiemnhnuykglnperfguwskbsaoxpmwegydtjtayzeptvoreosenwyidtbfsrnoxhylkptiobglfzszointnmojplucyjsuebknnambddtahtbonrpkbsnfrenmoutrylbdpktlulkmkaxplvldeascwhdzsqddkvezstbkpmwgolplalufdehtsrffhwkuewtmngrknntvwkotdihlntoswgrhscmgsataeaeaefzfpfwfxfyfefgflgdcyvybdhkgwasvoimkbmhdmsbtihnammegsgdgygmgurtsesasrssskswstcfnbpdct"
        )

        let parsed = try EncryptedMessage.fromURString(ur)
        XCTAssertEqual(parsed, encrypted)
    }
}
