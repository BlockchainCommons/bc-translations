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
}
