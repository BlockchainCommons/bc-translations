import BCComponents
import DCBOR
import XCTest

@MainActor
final class NonceJSONTests: XCTestCase {
    func testNonceHexRoundtrip() throws {
        let nonce = Nonce.new()
        let hex = nonce.hex()
        let parsed = try Nonce.fromHex(hex)
        XCTAssertEqual(parsed, nonce)
    }

    func testNonceCBORRoundtrip() throws {
        let nonce = Nonce.new()
        let cbor = nonce.taggedCBOR
        let parsed = try Nonce(cbor: cbor)
        XCTAssertEqual(parsed, nonce)
    }

    func testJSONRoundtrip() throws {
        let json = JSON.fromString("{\"name\":\"Alice\"}")
        let cbor = json.taggedCBOR
        let parsed = try JSON(cbor: cbor)
        XCTAssertEqual(parsed, json)
        XCTAssertEqual(parsed.asString(), "{\"name\":\"Alice\"}")
    }
}
