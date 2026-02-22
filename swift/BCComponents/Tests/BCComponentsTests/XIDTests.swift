import BCComponents
import XCTest

@MainActor
final class XIDTests: XCTestCase {
    func testXIDBasic() throws {
        registerTags()

        let xid = try XID.fromData(
            hexData("de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037")
        )
        XCTAssertEqual(
            xid.toHex(),
            "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037"
        )
        XCTAssertEqual(xid.shortDescription(), "de285368")
        XCTAssertEqual(
            xid.urString(),
            "ur:xid/hdcxuedeguisgevwhdaxnbluenutlbglhfiygamsamadmojkdydtneteeowffhwprtemcaatledk"
        )
        XCTAssertEqual(xid.bytewordsIdentifier(prefix: true), "🅧 URGE DICE GURU IRIS")
        XCTAssertEqual(xid.bytemojiIdentifier(prefix: true), "🅧 🐻 😻 🍞 💐")
    }

    func testXIDFromSchnorrKey() throws {
        registerTags()

        let privateKey = SigningPrivateKey.newSchnorr(
            try ECPrivateKey(
                hexData("322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36")
            )
        )
        let publicKey = try privateKey.publicKey()

        let keyCBOR = publicKey.taggedCBOR
        XCTAssertEqual(
            keyCBOR.cborData,
            hexData("d99c565820e8251dc3a17e0f2c07865ed191139ecbcddcbdd070ec1ff65df5148c7ef4005a")
        )

        let digest = Digest.fromImage(keyCBOR.cborData)
        XCTAssertEqual(
            digest.data,
            hexData("d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47")
        )

        let xid = XID.new(genesisKey: publicKey)
        XCTAssertEqual(
            xid.toHex(),
            "d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47"
        )
        XCTAssertTrue(xid.validate(genesisKey: publicKey))

        XCTAssertEqual(xid.description, "XID(d40e0602)")
        let reference = xid.reference()
        XCTAssertEqual(reference.description, "Reference(d40e0602)")
        XCTAssertEqual(reference.bytewordsIdentifier(nil), "TINY BETA ATOM ALSO")
        XCTAssertEqual(reference.bytemojiIdentifier(nil), "🧦 🤨 😎 😆")
    }
}
