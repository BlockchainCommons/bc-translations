import Testing
@testable import BCUR

struct Crc32Tests {
    @Test func testCrc32() {
        #expect(Crc32.checksum(Array("Hello, world!".utf8)) == 0xEBE6_C6E6)
        #expect(Crc32.checksum(Array("Wolf".utf8)) == 0x598C_84DC)
    }
}
