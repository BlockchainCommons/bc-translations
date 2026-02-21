import Testing
import Foundation
import BCRand
@testable import BCShamir

struct FakeRandomNumberGenerator: BCRandomNumberGenerator {
    mutating func nextUInt32() -> UInt32 {
        fatalError("not implemented")
    }

    mutating func nextUInt64() -> UInt64 {
        fatalError("not implemented")
    }

    mutating func randomData(count: Int) -> Data {
        var data = Data(repeating: 0, count: count)
        fillRandomData(&data)
        return data
    }

    mutating func fillRandomData(_ data: inout Data) {
        var b: UInt8 = 0
        for i in 0..<data.count {
            data[i] = b
            b = b &+ 17
        }
    }
}

private func hexToBytes(_ hex: String) -> [UInt8] {
    var bytes = [UInt8]()
    var index = hex.startIndex
    while index < hex.endIndex {
        let nextIndex = hex.index(index, offsetBy: 2)
        let byteString = hex[index..<nextIndex]
        bytes.append(UInt8(byteString, radix: 16)!)
        index = nextIndex
    }
    return bytes
}

@Test func testSplitSecret3of5() throws {
    var rng = FakeRandomNumberGenerator()
    let secret = hexToBytes("0ff784df000c4380a5ed683f7e6e3dcf")
    let shares = try splitSecret(threshold: 3, shareCount: 5, secret: secret, randomGenerator: &rng)
    #expect(shares.count == 5)
    #expect(shares[0] == hexToBytes("00112233445566778899aabbccddeeff"))
    #expect(shares[1] == hexToBytes("d43099fe444807c46921a4f33a2a798b"))
    #expect(shares[2] == hexToBytes("d9ad4e3bec2e1a7485698823abf05d36"))
    #expect(shares[3] == hexToBytes("0d8cf5f6ec337bc764d1866b5d07ca42"))
    #expect(shares[4] == hexToBytes("1aa7fe3199bc5092ef3816b074cabdf2"))

    let recoveredShareIndices = [1, 2, 4]
    let recoveredShares = recoveredShareIndices.map { shares[$0] }
    let recoveredSecret = try recoverSecret(indices: recoveredShareIndices, shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func testSplitSecret2of7() throws {
    var rng = FakeRandomNumberGenerator()
    let secret = hexToBytes("204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a")
    let shares = try splitSecret(threshold: 2, shareCount: 7, secret: secret, randomGenerator: &rng)
    #expect(shares.count == 7)
    #expect(shares[0] == hexToBytes("2dcd14c2252dc8489af3985030e74d5a48e8eff1478ab86e65b43869bf39d556"))
    #expect(shares[1] == hexToBytes("a1dfdd798388aada635b9974472b4fc59a32ae520c42c9f6a0af70149b882487"))
    #expect(shares[2] == hexToBytes("2ee99daf727c0c7773b89a18de64497ff7476dacd1015a45f482a893f7402cef"))
    #expect(shares[3] == hexToBytes("a2fb5414d4d96ee58a109b3ca9a84be0259d2c0f9ac92bdd3199e0eed3f1dd3e"))
    #expect(shares[4] == hexToBytes("2b851d188b8f5b3653659cc0f7fa45102dadf04b708767385cd803862fcb3c3f"))
    #expect(shares[5] == hexToBytes("a797d4a32d2a39a4aacd9de48036478fff77b1e83b4f16a099c34bfb0b7acdee"))
    #expect(shares[6] == hexToBytes("28a19475dcde9f09ba2e9e881979413592027216e60c8513cdee937c67b2c586"))

    let recoveredShareIndices = [3, 4]
    let recoveredShares = recoveredShareIndices.map { shares[$0] }
    let recoveredSecret = try recoverSecret(indices: recoveredShareIndices, shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func testExampleSplit() throws {
    let threshold = 2
    let shareCount = 3
    let secret: [UInt8] = Array("my secret belongs to me.".utf8)
    var randomGenerator = SecureRandomNumberGenerator()
    let shares = try splitSecret(
        threshold: threshold,
        shareCount: shareCount,
        secret: secret,
        randomGenerator: &randomGenerator
    )
    #expect(shares.count == shareCount)
}

@Test func testExampleRecover() throws {
    let indices = [0, 2]
    let shares: [[UInt8]] = [
        [
            47, 165, 102, 232, 218, 99, 6, 94, 39, 6, 253, 215, 12, 88, 64, 32,
            105, 40, 222, 146, 93, 197, 48, 129,
        ],
        [
            221, 174, 116, 201, 90, 99, 136, 33, 64, 215, 60, 84, 207, 28, 74,
            10, 111, 243, 43, 224, 48, 64, 199, 172,
        ],
    ]

    let secret = try recoverSecret(indices: indices, shares: shares)
    #expect(secret == Array("my secret belongs to me.".utf8))
}
