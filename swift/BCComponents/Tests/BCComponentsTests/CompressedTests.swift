@testable import BCComponents
import XCTest

final class CompressedTests: XCTestCase {
    func testCompressedLongText() throws {
        let source = Data(
            "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium.".utf8
        )
        let compressed = Compressed.fromDecompressedData(source, digest: nil)
        XCTAssertEqual(
            compressed.debugDescription,
            "Compressed(checksum: 3eeb10a0, size: 217/364, ratio: 0.60, digest: None)"
        )
        XCTAssertEqual(try compressed.decompress(), source)
    }

    func testCompressedShortText() throws {
        let source = Data("Lorem ipsum dolor sit amet consectetur adipiscing".utf8)
        let compressed = Compressed.fromDecompressedData(source, digest: nil)
        XCTAssertEqual(
            compressed.debugDescription,
            "Compressed(checksum: 29db1793, size: 47/49, ratio: 0.96, digest: None)"
        )
        XCTAssertEqual(try compressed.decompress(), source)
    }

    func testCompressedTinyText() throws {
        let source = Data("Lorem".utf8)
        let compressed = Compressed.fromDecompressedData(source, digest: nil)
        XCTAssertEqual(
            compressed.debugDescription,
            "Compressed(checksum: 44989b39, size: 5/5, ratio: 1.00, digest: None)"
        )
        XCTAssertEqual(try compressed.decompress(), source)
    }

    func testCompressedEmpty() throws {
        let source = Data()
        let compressed = Compressed.fromDecompressedData(source, digest: nil)
        XCTAssertEqual(
            compressed.debugDescription,
            "Compressed(checksum: 00000000, size: 0/0, ratio: NaN, digest: None)"
        )
        XCTAssertEqual(try compressed.decompress(), source)
    }
}
