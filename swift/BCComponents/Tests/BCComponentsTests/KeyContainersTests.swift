import BCComponents
import BCTags
import DCBOR
import XCTest

@MainActor
final class KeyContainersTests: XCTestCase {
    private let seed = hexData("59f2293a5bce7d4de59e71b4207ac5d2")

    func testPrivateKeyBaseVectors() throws {
        BCTags.registerTags()

        let privateKeyBase = PrivateKeyBase.fromData(seed)

        XCTAssertEqual(
            privateKeyBase.ecdsaSigningPrivateKey().toEcdsa()?.data,
            hexData("9505a44aaf385ce633cf0e2bc49e65cc88794213bdfbf8caf04150b9c4905f5a")
        )
        XCTAssertEqual(
            try privateKeyBase.schnorrSigningPrivateKey().publicKey().toSchnorr()?.data,
            hexData("fd4d22f9e8493da52d730aa402ac9e661deca099ef4db5503f519a73c3493e18")
        )
        XCTAssertEqual(
            privateKeyBase.x25519PrivateKey().data,
            hexData("77ff838285a0403d3618aa8c30491f99f55221be0b944f50bfb371f43b897485")
        )
        XCTAssertEqual(
            privateKeyBase.x25519PrivateKey().publicKey().data,
            hexData("863cf3facee3ba45dc54e5eedecb21d791d64adfb0a1c63bfb6fea366c1ee62b")
        )

        let ur = privateKeyBase.urString()
        XCTAssertEqual(
            ur,
            "ur:crypto-prvkey-base/gdhkwzdtfthptokigtvwnnjsqzcxknsktdsfecsbbk"
        )
        XCTAssertEqual(try PrivateKeyBase.fromURString(ur), privateKeyBase)
    }

    func testPrivateKeysRoundtripAndVectors() throws {
        BCTags.registerTags()

        let privateKeys = PrivateKeyBase.fromData(seed).privateKeys()

        let cbor = privateKeys.cbor
        let decoded = try PrivateKeys(cbor: cbor)
        XCTAssertEqual(privateKeys, decoded)
        XCTAssertEqual(cbor, decoded.cbor)

        let ur = privateKeys.urString()
        XCTAssertEqual(
            ur,
            "ur:crypto-prvkeys/lftansgohdcxmdahoxgepeethhvaeotkbadnssnnihsflokkfwbwryzoyasgwtfpgdrhssmhhehttansgehdcxktzmlslflpnbfzfsencspklkdygactnlykgmclrnbdmwgwgdrsqdjswkfrldjylpmtdpskfx"
        )
        XCTAssertEqual(try PrivateKeys.fromURString(ur), privateKeys)

        XCTAssertEqual(
            privateKeys.description,
            "PrivateKeys(fa742ac8, SigningPrivateKey(2a645922, SchnorrPrivateKey(0b02c820)), EncapsulationPrivateKey(ded5f016, X25519PrivateKey(ded5f016)))"
        )
        XCTAssertEqual(
            privateKeys.reference().description,
            "Reference(fa742ac8)"
        )
    }

    func testPublicKeysRoundtripAndVectors() throws {
        BCTags.registerTags()

        let publicKeys = PrivateKeyBase.fromData(seed).publicKeys()

        let cbor = publicKeys.cbor
        let decoded = try PublicKeys(cbor: cbor)
        XCTAssertEqual(publicKeys, decoded)
        XCTAssertEqual(cbor, decoded.cbor)

        let ur = publicKeys.urString()
        XCTAssertEqual(
            ur,
            "ur:crypto-pubkeys/lftanshfhdcxzcgtcpytvsgafsondpjkbkoxaopsnniycawpnbnlwsgtregdfhgynyjksrgafmcstansgrhdcxlnfnwfzstovlrdfeuoghvwwyuesbcltsmetbgeurpfoyswfrzojlwdenjzckvadnrndtgsya"
        )
        XCTAssertEqual(try PublicKeys.fromURString(ur), publicKeys)

        XCTAssertEqual(
            publicKeys.description,
            "PublicKeys(c9ede672, SigningPublicKey(7efa2ea1, SchnorrPublicKey(b4df96ce)), EncapsulationPublicKey(bacae62f, X25519PublicKey(bacae62f)))"
        )
        XCTAssertEqual(
            publicKeys.reference().description,
            "Reference(c9ede672)"
        )
    }
}
