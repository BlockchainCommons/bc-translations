import Testing
@testable import ProvenanceMark

struct Xoshiro256StarStarTests {
    @Test func rng() {
        let data = Array("Hello World".utf8)
        let digest = CryptoUtils.sha256(data)
        var rng = Xoshiro256StarStar(data: digest)
        let key = rng.nextBytes(count: 32)
        let expected: [UInt8] = [
            0xb1, 0x8b, 0x44, 0x6d, 0xf4, 0x14, 0xec, 0x00,
            0x71, 0x4f, 0x19, 0xcb, 0x0f, 0x03, 0xe4, 0x5c,
            0xd3, 0xc3, 0xd5, 0xd0, 0x71, 0xd2, 0xe7, 0x48,
            0x3b, 0xa8, 0x62, 0x7c, 0x65, 0xb9, 0x92, 0x6a,
        ]
        #expect(key == expected)
    }

    @Test func saveRngState() {
        let state: [UInt64] = [
            17295166580085024720,
            422929670265678780,
            5577237070365765850,
            7953171132032326923,
        ]
        let data = Xoshiro256StarStar(state: state).data
        let expected: [UInt8] = [
            0xd0, 0xe7, 0x2c, 0xf1, 0x5e, 0xc6, 0x04, 0xf0,
            0xbc, 0xab, 0x28, 0x59, 0x4b, 0x8c, 0xde, 0x05,
            0xda, 0xb0, 0x4a, 0xe7, 0x90, 0x53, 0x66, 0x4d,
            0x0b, 0x9d, 0xad, 0xc2, 0x01, 0x57, 0x5f, 0x6e,
        ]
        #expect(data == expected)

        let state2 = Xoshiro256StarStar(data: data).state
        let data2 = Xoshiro256StarStar(state: state2).data
        #expect(data == data2)
    }
}
