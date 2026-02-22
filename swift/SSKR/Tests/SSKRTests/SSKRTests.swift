import Foundation
import Testing

import BCRand
@testable import SSKR

// MARK: - Test Helpers

/// Deterministic RNG for testing: fills bytes starting at 0, incrementing by 17
/// (wrapping at 256). This matches the Rust test's FakeRandomNumberGenerator.
struct FakeRandomNumberGenerator: BCRandomNumberGenerator {
    mutating func nextUInt32() -> UInt32 {
        fatalError("not implemented")
    }

    mutating func nextUInt64() -> UInt64 {
        fatalError("not implemented")
    }

    mutating func randomData(count: Int) -> Data {
        var data = Data(count: count)
        fillRandomData(&data)
        return data
    }

    mutating func fillRandomData(_ data: inout Data) {
        var b: UInt8 = 0
        for i in data.indices {
            data[i] = b
            b = b &+ 17
        }
    }
}

/// Hex string to byte array conversion.
func hexToBytes(_ hex: String) -> [UInt8] {
    var bytes: [UInt8] = []
    var index = hex.startIndex
    while index < hex.endIndex {
        let nextIndex = hex.index(index, offsetBy: 2)
        let byteString = hex[index..<nextIndex]
        bytes.append(UInt8(byteString, radix: 16)!)
        index = nextIndex
    }
    return bytes
}

/// Fisher-Yates shuffle matching the Rust implementation.
func fisherYatesShuffle<T>(_ slice: inout [T], rng: inout some BCRandomNumberGenerator) {
    var i = slice.count
    while i > 1 {
        i -= 1
        let j = rngNextInClosedRange(&rng, range: 0...i)
        slice.swapAt(i, j)
    }
}

/// Test harness for recovery: generates shares, selects a random quorum,
/// and verifies that recovery produces the original secret.
struct RecoverSpec {
    let secret: Secret
    let spec: Spec
    let shares: [[[UInt8]]]
    let recoveredGroupIndices: [Int]
    let recoveredMemberIndices: [[Int]]
    let recoveredShares: [[UInt8]]

    init(
        secret: Secret,
        spec: Spec,
        shares: [[[UInt8]]],
        rng: inout some BCRandomNumberGenerator
    ) {
        var groupIndices = Array(0..<spec.groupCount)
        fisherYatesShuffle(&groupIndices, rng: &rng)
        let recoveredGroupIndices = Array(groupIndices[..<spec.groupThreshold])

        var recoveredMemberIndices: [[Int]] = []
        for groupIndex in recoveredGroupIndices {
            let group = spec.groups[groupIndex]
            var memberIndices = Array(0..<group.memberCount)
            fisherYatesShuffle(&memberIndices, rng: &rng)
            let recoveredMemberIndicesForGroup =
                Array(memberIndices[..<group.memberThreshold])
            recoveredMemberIndices.append(recoveredMemberIndicesForGroup)
        }

        var recoveredShares: [[UInt8]] = []
        for (i, recoveredGroupIndex) in recoveredGroupIndices.enumerated() {
            let groupShares = shares[recoveredGroupIndex]
            for recoveredMemberIndex in recoveredMemberIndices[i] {
                let memberShare = groupShares[recoveredMemberIndex]
                recoveredShares.append(memberShare)
            }
        }
        fisherYatesShuffle(&recoveredShares, rng: &rng)

        self.secret = secret
        self.spec = spec
        self.shares = shares
        self.recoveredGroupIndices = recoveredGroupIndices
        self.recoveredMemberIndices = recoveredMemberIndices
        self.recoveredShares = recoveredShares
    }

    func recover() throws {
        let recoveredSecret = try sskrCombine(shares: recoveredShares)
        #expect(recoveredSecret == secret)
    }
}

func oneFuzzTest(rng: inout some BCRandomNumberGenerator) throws {
    let secretLen =
        rngNextInClosedRange(&rng, range: minSecretLen...maxSecretLen) & ~1
    let secretData = [UInt8](rng.randomData(count: secretLen))
    let secret = try Secret(secretData)
    let groupCount = rngNextInClosedRange(&rng, range: 1...maxGroupsCount)
    let groupSpecs: [GroupSpec] = try (0..<groupCount).map { _ in
        let memberCount = rngNextInClosedRange(&rng, range: 1...maxShareCount)
        let memberThreshold = rngNextInClosedRange(&rng, range: 1...memberCount)
        return try GroupSpec(
            memberThreshold: memberThreshold,
            memberCount: memberCount
        )
    }
    let groupThreshold = rngNextInClosedRange(&rng, range: 1...groupCount)
    let spec = try Spec(groupThreshold: groupThreshold, groups: groupSpecs)
    let shares = try sskrGenerateUsing(
        spec: spec,
        secret: secret,
        randomGenerator: &rng
    )

    let recoverSpec = RecoverSpec(
        secret: secret,
        spec: spec,
        shares: shares,
        rng: &rng
    )
    try recoverSpec.recover()
}

// MARK: - Tests

@Test func testSplit3Of5() throws {
    var rng = FakeRandomNumberGenerator()
    let secret = try Secret(hexToBytes("0ff784df000c4380a5ed683f7e6e3dcf"))
    let group = try GroupSpec(memberThreshold: 3, memberCount: 5)
    let spec = try Spec(groupThreshold: 1, groups: [group])
    let shares = try sskrGenerateUsing(
        spec: spec,
        secret: secret,
        randomGenerator: &rng
    )
    let flattenedShares = shares.flatMap { $0 }
    #expect(flattenedShares.count == 5)
    for share in flattenedShares {
        #expect(share.count == metadataSizeBytes + secret.count)
    }

    let recoveredShareIndices = [1, 2, 4]
    let recoveredShares = recoveredShareIndices.map { flattenedShares[$0] }
    let recoveredSecret = try sskrCombine(shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func testSplit2Of7() throws {
    var rng = FakeRandomNumberGenerator()
    let secret = try Secret(hexToBytes(
        "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"
    ))
    let group = try GroupSpec(memberThreshold: 2, memberCount: 7)
    let spec = try Spec(groupThreshold: 1, groups: [group])
    let shares = try sskrGenerateUsing(
        spec: spec,
        secret: secret,
        randomGenerator: &rng
    )
    #expect(shares.count == 1)
    #expect(shares[0].count == 7)
    let flattenedShares = shares.flatMap { $0 }
    #expect(flattenedShares.count == 7)
    for share in flattenedShares {
        #expect(share.count == metadataSizeBytes + secret.count)
    }

    let recoveredShareIndices = [3, 4]
    let recoveredShares = recoveredShareIndices.map { flattenedShares[$0] }
    let recoveredSecret = try sskrCombine(shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func testSplit2Of3_2Of3() throws {
    var rng = FakeRandomNumberGenerator()
    let secret = try Secret(hexToBytes(
        "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"
    ))
    let group1 = try GroupSpec(memberThreshold: 2, memberCount: 3)
    let group2 = try GroupSpec(memberThreshold: 2, memberCount: 3)
    let spec = try Spec(groupThreshold: 2, groups: [group1, group2])
    let shares = try sskrGenerateUsing(
        spec: spec,
        secret: secret,
        randomGenerator: &rng
    )
    #expect(shares.count == 2)
    #expect(shares[0].count == 3)
    #expect(shares[1].count == 3)
    let flattenedShares = shares.flatMap { $0 }
    #expect(flattenedShares.count == 6)
    for share in flattenedShares {
        #expect(share.count == metadataSizeBytes + secret.count)
    }

    let recoveredShareIndices = [0, 1, 3, 5]
    let recoveredShares = recoveredShareIndices.map { flattenedShares[$0] }
    let recoveredSecret = try sskrCombine(shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func testShuffle() throws {
    var rng = makeFakeRandomNumberGenerator()
    var v = Array(0..<100)
    fisherYatesShuffle(&v, rng: &rng)
    #expect(v.count == 100)
    #expect(v == [
        79, 70, 40, 53, 25, 30, 31, 88, 10, 1, 45, 54, 81, 58, 55, 59,
        69, 78, 65, 47, 75, 61, 0, 72, 20, 9, 80, 13, 73, 11, 60, 56,
        19, 42, 33, 12, 36, 38, 6, 35, 68, 77, 50, 18, 97, 49, 98, 85,
        89, 91, 15, 71, 99, 67, 84, 23, 64, 14, 57, 48, 62, 29, 28, 94,
        44, 8, 66, 34, 43, 21, 63, 16, 92, 95, 27, 51, 26, 86, 22, 41,
        93, 82, 7, 87, 74, 37, 46, 3, 96, 24, 90, 39, 32, 17, 76, 4,
        83, 2, 52, 5,
    ])
}

@Test func fuzzTest() throws {
    var rng = makeFakeRandomNumberGenerator()
    for _ in 0..<100 {
        try oneFuzzTest(rng: &rng)
    }
}

@Test func exampleEncode() throws {
    let secretString = Array("my secret belongs to me.".utf8)
    let secret = try Secret(secretString)

    let group1 = try GroupSpec(memberThreshold: 2, memberCount: 3)
    let group2 = try GroupSpec(memberThreshold: 3, memberCount: 5)
    let spec = try Spec(groupThreshold: 2, groups: [group1, group2])

    let shares = try sskrGenerate(spec: spec, secret: secret)

    #expect(shares.count == 2)
    #expect(shares[0].count == 3)
    #expect(shares[1].count == 5)

    let recoveredShares = [
        shares[0][0],
        shares[0][2],
        shares[1][0],
        shares[1][1],
        shares[1][4],
    ]

    let recoveredSecret = try sskrCombine(shares: recoveredShares)
    #expect(recoveredSecret == secret)
}

@Test func exampleEncode3() throws {
    let text = "my secret belongs to me."

    func roundtrip(m: Int, n: Int) throws -> Secret {
        let secret = try Secret(Array(text.utf8))
        let spec = try Spec(
            groupThreshold: 1,
            groups: [GroupSpec(memberThreshold: m, memberCount: n)]
        )
        let shares = try sskrGenerate(spec: spec, secret: secret)
        let flatShares = shares.flatMap { $0 }
        return try sskrCombine(shares: flatShares)
    }

    // Good, uses a 2/3 group
    do {
        let result = try roundtrip(m: 2, n: 3)
        #expect(String(bytes: result.data, encoding: .utf8) == text)
    }

    // Still ok, uses a 1/1 group
    do {
        let result = try roundtrip(m: 1, n: 1)
        #expect(String(bytes: result.data, encoding: .utf8) == text)
    }

    // Fixed, uses a 1/3 group
    do {
        let result = try roundtrip(m: 1, n: 3)
        #expect(String(bytes: result.data, encoding: .utf8) == text)
    }
}

@Test func exampleEncode4() throws {
    let text = "my secret belongs to me."
    let secret = try Secret(Array(text.utf8))
    let spec = try Spec(
        groupThreshold: 1,
        groups: [
            GroupSpec(memberThreshold: 2, memberCount: 3),
            GroupSpec(memberThreshold: 2, memberCount: 3),
        ]
    )
    let groupedShares = try sskrGenerate(spec: spec, secret: secret)
    let flattenedShares = groupedShares.flatMap { $0 }

    // The group threshold is 1, but we're providing an additional share
    // from the second group. This was previously causing an error,
    // because the second group could not be decoded. The correct
    // behavior is to ignore any group's shares that cannot be decoded.
    let recoveredShareIndices = [0, 1, 3]
    let recoveredShares = recoveredShareIndices.map { flattenedShares[$0] }
    let recoveredSecret = try sskrCombine(shares: recoveredShares)
    #expect(String(bytes: recoveredSecret.data, encoding: .utf8) == text)
}
