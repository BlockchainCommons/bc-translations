import BCComponents
import XCTest

@MainActor
final class JSONTests: XCTestCase {
    func testJSONCreation() {
        let json = JSON.fromString(#"{"key": "value"}"#)
        XCTAssertEqual(json.stringValue, #"{"key": "value"}"#)
        XCTAssertEqual(json.count, 16)
        XCTAssertFalse(json.isEmpty)
    }

    func testJSONFromBytes() {
        let data = Data("[1, 2, 3]".utf8)
        let json = JSON(data)
        XCTAssertEqual(json.data, data)
        XCTAssertEqual(json.stringValue, "[1, 2, 3]")
    }

    func testJSONEmpty() {
        let json = JSON.fromString("")
        XCTAssertTrue(json.isEmpty)
        XCTAssertEqual(json.count, 0)
    }

    func testJSONCBORRoundtrip() throws {
        let json = JSON.fromString(#"{"name":"Alice","age":30}"#)
        let decoded = try JSON(cbor: json.taggedCBOR)
        XCTAssertEqual(json, decoded)
    }

    func testJSONHexRoundtrip() throws {
        let json = JSON.fromString("test")
        let decoded = try JSON.fromHex(json.hex)
        XCTAssertEqual(json, decoded)
    }

    func testJSONDebug() {
        let json = JSON.fromString(#"{"test":true}"#)
        XCTAssertEqual(json.debugDescription, #"JSON({"test":true})"#)
    }

    func testJSONCloneLikeCopy() {
        let json = JSON.fromString("original")
        let copy = json
        XCTAssertEqual(json, copy)
    }

    func testJSONData() {
        let json = JSON.fromString("data")
        XCTAssertEqual(json.data, Data("data".utf8))
    }
}
