import BCComponents
import BCRand
import XCTest

@MainActor
final class X25519Tests: XCTestCase {
    func testX25519VectorsAndSharedKey() throws {
        registerTags()

        var rng = makeFakeRandomNumberGenerator()
        let privateKey = X25519PrivateKey.newUsing(rng: &rng)
        XCTAssertEqual(
            privateKey.urString(),
            "ur:agreement-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct"
        )

        let parsedPrivate = try X25519PrivateKey.fromURString(privateKey.urString())
        XCTAssertEqual(parsedPrivate, privateKey)

        let publicKey = privateKey.publicKey()
        XCTAssertEqual(
            publicKey.urString(),
            "ur:agreement-public-key/hdcxwnryknkbbymnoxhswmptgydsotwswsghfmrkksfxntbzjyrnuornkildchgswtdahehpwkrl"
        )

        let parsedPublic = try X25519PublicKey.fromURString(publicKey.urString())
        XCTAssertEqual(parsedPublic, publicKey)

        let derived = X25519PrivateKey.deriveFromKeyMaterial(Data("password".utf8))
        XCTAssertEqual(
            derived.urString(),
            "ur:agreement-private-key/hdcxkgcfkomeeyiemywkftvabnrdolmttlrnfhjnguvaiehlrldmdpemgyjlatdthsnecytdoxat"
        )

        var rng2 = makeFakeRandomNumberGenerator()
        let alicePrivate = X25519PrivateKey.newUsing(rng: &rng2)
        let bobPrivate = X25519PrivateKey.newUsing(rng: &rng2)
        let aliceShared = alicePrivate.sharedKey(with: bobPrivate.publicKey())
        let bobShared = bobPrivate.sharedKey(with: alicePrivate.publicKey())
        XCTAssertEqual(aliceShared, bobShared)
    }
}
