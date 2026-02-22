import BCComponents
import XCTest

@MainActor
final class DigestTests: XCTestCase {
    func testDigestVectorAndUR() throws {
        registerTags()

        let digest = Digest.fromImage(Data("hello world".utf8))
        XCTAssertEqual(
            digest.hex(),
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
}
