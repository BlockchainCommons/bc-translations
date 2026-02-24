import Testing
import BCComponents
import BCEnvelope
import WolfBase
import Foundation

struct TypeTests {
    @Test func testKnownValue() throws {
        let envelope = try Envelope(.signed).checkEncoding()
        #expect(envelope.description == ".knownValue(signed)")
        #expect(envelope.digest† == "Digest(d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c)")
        #expect(envelope.format() == "'signed'")
        #expect(envelope.urString == "ur:envelope/axgrbdrnem")
    }

    @Test func testDate() throws {
        let envelope = try Envelope(Date(iso8601: "2018-01-07")).checkEncoding()
        #expect(envelope.format() == "2018-01-07")
//        print(envelope.diagnostic())
        
        let _ = try Envelope(Date(timeIntervalSince1970: 1693454262.5)).checkEncoding()
//        print(e.format())
//        print(e.diagnostic())
    }
    
    @Test func testFakeRandomData() {
        #expect(
            fakeRandomData(count: 100) ==
            ‡"7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d3545532daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a564e59b4e2"
        )
    }
    
    @Test func testFakeNumbers() {
        var rng = makeFakeRandomNumberGenerator()
        let array = (0..<100).map { _ in
            rngNextInClosedRange(&rng, range: -50...50, bits: 32)
        }
        #expect(
            String(describing: array) ==
            "[-43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46, 20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23, 24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17, -15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38, -29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42, -29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46, -19, 16, 2, -45, 41, 12, -22, 43, -11]"
        )
    }
}
