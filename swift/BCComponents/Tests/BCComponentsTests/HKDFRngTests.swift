@testable import BCComponents
import XCTest

final class HKDFRngTests: XCTestCase {
    func testHKDFRngNew() {
        let rng = HKDFRng(keyMaterial: Data("key_material".utf8), salt: "salt")
        XCTAssertEqual(rng.keyMaterial, Data("key_material".utf8))
        XCTAssertEqual(rng.salt, "salt")
        XCTAssertEqual(rng.pageLength, 32)
        XCTAssertEqual(rng.pageIndex, 0)
        XCTAssertTrue(rng.buffer.isEmpty)
        XCTAssertEqual(rng.position, 0)
    }

    func testHKDFRngFillBufferBehavior() {
        var rng = HKDFRng(
            keyMaterial: Data("key_material".utf8),
            salt: "salt",
            pageLength: 16
        )
        _ = rng.nextBytes(length: 1)
        XCTAssertFalse(rng.buffer.isEmpty)
        XCTAssertEqual(rng.pageIndex, 1)
    }

    func testHKDFRngNextBytes() {
        var rng = HKDFRng(keyMaterial: Data("key_material".utf8), salt: "salt")
        XCTAssertEqual(hexEncode(rng.nextBytes(length: 16)), "1032ac8ffea232a27c79fe381d7eb7e4")
        XCTAssertEqual(hexEncode(rng.nextBytes(length: 16)), "aeaaf727d35b6f338218391f9f8fa1f3")
        XCTAssertEqual(hexEncode(rng.nextBytes(length: 16)), "4348a59427711deb1e7d8a6959c6adb4")
        XCTAssertEqual(hexEncode(rng.nextBytes(length: 16)), "5d937a42cb5fb090fe1a1ec88f56e32b")
    }

    func testHKDFRngNextUInt32() {
        var rng = HKDFRng(keyMaterial: Data("key_material".utf8), salt: "salt")
        XCTAssertEqual(rng.nextUInt32(), 2410426896)
    }

    func testHKDFRngNextUInt64() {
        var rng = HKDFRng(keyMaterial: Data("key_material".utf8), salt: "salt")
        XCTAssertEqual(rng.nextUInt64(), 11687583197195678224)
    }

    func testHKDFRngFillRandomData() {
        var rng = HKDFRng(keyMaterial: Data("key_material".utf8), salt: "salt")
        var dest = Data(count: 16)
        rng.fillRandomData(&dest)
        XCTAssertEqual(hexEncode(dest), "1032ac8ffea232a27c79fe381d7eb7e4")
    }
}
