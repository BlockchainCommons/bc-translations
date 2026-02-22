import Foundation
import Testing
import DCBOR
@testable import BCUR

struct ExampleTests {
    @Test func testEncode() throws {
        let cbor: CBOR = [1, 2, 3]
        let ur = try UR("test", cbor)
        #expect(ur.urString == "ur:test/lsadaoaxjygonesw")
    }

    @Test func testDecode() throws {
        let ur = try UR(urString:"ur:test/lsadaoaxjygonesw")
        #expect(ur.urTypeString == "test")
        #expect(ur.cbor == ([1, 2, 3] as CBOR))
    }

    @Test func testFountain() throws {
        #expect(try runFountainTest(startPart: 1) == 5)
        #expect(try runFountainTest(startPart: 51) == 61)
        #expect(try runFountainTest(startPart: 101) == 110)
        #expect(try runFountainTest(startPart: 501) == 507)
    }

    private func runFountainTest(startPart: Int) throws -> Int {
        let message = "The only thing we have to fear is fear itself."
        let cbor = CBOR.bytes(Data(message.utf8))
        let ur = try UR("bytes", cbor)

        let encoder = try MultipartEncoder(ur, 10)
        let decoder = MultipartDecoder()

        for _ in 0..<1_000 {
            let part = try encoder.nextPart()
            if encoder.currentIndex >= startPart {
                try decoder.receive(part)
            }
            if decoder.isComplete {
                break
            }
        }

        #expect(try decoder.message() == ur)
        return encoder.currentIndex
    }
}
