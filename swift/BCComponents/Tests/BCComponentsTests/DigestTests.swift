import BCComponents
import XCTest

@MainActor
final class DigestTests: XCTestCase {
    func testDigestVectorAndUR() throws {
        registerTags()

        let digest = Digest.fromImage(Data("hello world".utf8))
        XCTAssertEqual(
            digest.hex,
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )

        let ur = digest.urString()
        XCTAssertEqual(
            ur,
            "ur:digest/hdcxrhgtdirhmugtfmayondmgmtstnkipyzssslrwsvlkngulawymhloylpsvowssnwlamnlatrs"
        )

        let decoded = try Digest.fromURString(ur)
        XCTAssertEqual(decoded, digest)
    }

    func testDigestFromHex() throws {
        let digest = try Digest.fromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )
        XCTAssertEqual(digest.data.count, Digest.digestSize)
        XCTAssertEqual(
            digest.hex,
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )
    }

    func testDigestEquality() throws {
        let digest1 = try Digest.fromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )
        let digest2 = try Digest.fromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )
        XCTAssertEqual(digest1, digest2)
    }

    func testDigestInequality() throws {
        let digest1 = try Digest.fromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
        )
        let digest3 = try Digest.fromHex(
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
        )
        XCTAssertNotEqual(digest1, digest3)
    }

    func testDigestValidateOpt() {
        let image = Data("hello world".utf8)
        let digest = Digest.fromImage(image)
        XCTAssertTrue(Digest.validateOpt(image: image, digest: digest))
        XCTAssertFalse(Digest.validateOpt(image: Data("other".utf8), digest: digest))
        XCTAssertTrue(Digest.validateOpt(image: image, digest: nil))
    }

    func testInvalidHexStringThrows() {
        XCTAssertThrowsError(try Digest.fromHex("invalid_hex_string"))
    }

    func testInvalidDigestURStringThrows() {
        XCTAssertThrowsError(try Digest.fromURString("ur:not_digest/invalid"))
    }
}
