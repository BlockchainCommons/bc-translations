import Foundation
import Testing
@testable import BCRand

let testSeed: (UInt64, UInt64, UInt64, UInt64) = (
    17295166580085024720,
    422929670265678780,
    5577237070365765850,
    7953171132032326923
)

@Test func testNextUInt64() {
    var rng = SeededRandomNumberGenerator(seed: testSeed)
    #expect(rng.nextUInt64() == 1104683000648959614)
}

@Test func testNext50() {
    var rng = SeededRandomNumberGenerator(seed: testSeed)
    let expectedValues: [UInt64] = [
        1104683000648959614,
        9817345228149227957,
        546276821344993881,
        15870950426333349563,
        830653509032165567,
        14772257893953840492,
        3512633850838187726,
        6358411077290857510,
        7897285047238174514,
        18314839336815726031,
        4978716052961022367,
        17373022694051233817,
        663115362299242570,
        9811238046242345451,
        8113787839071393872,
        16155047452816275860,
        673245095821315645,
        1610087492396736743,
        1749670338128618977,
        3927771759340679115,
        9610589375631783853,
        5311608497352460372,
        11014490817524419548,
        6320099928172676090,
        12513554919020212402,
        6823504187935853178,
        1215405011954300226,
        8109228150255944821,
        4122548551796094879,
        16544885818373129566,
        5597102191057004591,
        11690994260783567085,
        9374498734039011409,
        18246806104446739078,
        2337407889179712900,
        12608919248151905477,
        7641631838640172886,
        8421574250687361351,
        8697189342072434208,
        8766286633078002696,
        14800090277885439654,
        17865860059234099833,
        4673315107448681522,
        14288183874156623863,
        7587575203648284614,
        9109213819045273474,
        11817665411945280786,
        1745089530919138651,
        5730370365819793488,
        5496865518262805451,
    ]
    for expected in expectedValues {
        #expect(rng.nextUInt64() == expected)
    }
}

@Test func testFakeRandomData() {
    let data = fakeRandomData(count: 100)
    let expected = Data([
        0x7e, 0xb5, 0x59, 0xbb, 0xbf, 0x6c, 0xce, 0x26,
        0x32, 0xcf, 0x9f, 0x19, 0x4a, 0xeb, 0x50, 0x94,
        0x3d, 0xe7, 0xe1, 0xcb, 0xad, 0x54, 0xdc, 0xfa,
        0xb2, 0x7a, 0x42, 0x75, 0x9f, 0x5e, 0x2f, 0xed,
        0x51, 0x86, 0x84, 0xc5, 0x56, 0x47, 0x20, 0x08,
        0xa6, 0x79, 0x32, 0xf7, 0xc6, 0x82, 0x12, 0x5b,
        0x50, 0xcb, 0x72, 0xe8, 0x21, 0x6f, 0x69, 0x06,
        0x35, 0x8f, 0xda, 0xf2, 0x8d, 0x35, 0x45, 0x53,
        0x2d, 0xae, 0xe0, 0xc5, 0xbb, 0x50, 0x23, 0xf5,
        0x0c, 0xd8, 0xe7, 0x1e, 0xc1, 0x49, 0x01, 0xac,
        0x74, 0x6c, 0x57, 0x6c, 0x48, 0x1b, 0x89, 0x3b,
        0xe6, 0x65, 0x6b, 0x80, 0x62, 0x2b, 0x3a, 0x56,
        0x4e, 0x59, 0xb4, 0xe2,
    ])
    #expect(data == expected)
}

@Test func testNextWithUpperBound() {
    var rng = SeededRandomNumberGenerator(seed: testSeed)
    let result = rngNextWithUpperBound(&rng, upperBound: 10000, bits: 32)
    #expect(result == 745)
}

@Test func testInRange() {
    var rng = SeededRandomNumberGenerator(seed: testSeed)
    let values = (0..<100).map { _ in rngNextInRange(&rng, range: 0..<100, bits: 32) }
    let expected = [
        7, 44, 92, 16, 16, 67, 41, 74, 66, 20, 18, 6, 62, 34, 4, 69, 99,
        19, 0, 85, 22, 27, 56, 23, 19, 5, 23, 76, 80, 27, 74, 69, 17, 92,
        31, 32, 55, 36, 49, 23, 53, 2, 46, 6, 43, 66, 34, 71, 64, 69, 25,
        14, 17, 23, 32, 6, 23, 65, 35, 11, 21, 37, 58, 92, 98, 8, 38, 49,
        7, 24, 24, 71, 37, 63, 91, 21, 11, 66, 52, 54, 55, 19, 76, 46, 89,
        38, 91, 95, 33, 25, 4, 30, 66, 51, 5, 91, 62, 27, 92, 39,
    ]
    #expect(values == expected)
}

@Test func testFillRandomData() {
    var rng1 = SeededRandomNumberGenerator(seed: testSeed)
    let v1 = rng1.randomData(count: 100)
    var rng2 = SeededRandomNumberGenerator(seed: testSeed)
    var v2 = Data(count: 100)
    rng2.fillRandomData(&v2)
    #expect(v1 == v2)
}

@Test func testFakeNumbers() {
    var rng = makeFakeRandomNumberGenerator()
    let values = (0..<100).map { _ in rngNextInClosedRange(&rng, range: -50...50, bits: 32) }
    let expected = [
        -43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46,
        20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23,
        24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17,
        -15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38,
        -29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42,
        -29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46,
        -19, 16, 2, -45, 41, 12, -22, 43, -11,
    ]
    #expect(values == expected)
}

@Test func testRandomData() {
    let data1 = randomData(count: 32)
    let data2 = randomData(count: 32)
    let data3 = randomData(count: 32)
    #expect(data1.count == 32)
    #expect(data1 != data2)
    #expect(data1 != data3)
}
