import Foundation
import BCComponents

extension EnvelopeError {
    static let invalidShares = EnvelopeError("invalidShares")
}

public typealias RandomDataFunc = (Int) -> Data

public func secureRandomData(_ count: Int) -> Data {
    randomData(count: count)
}

private struct ClosureRandomNumberGenerator: BCRandomNumberGenerator {
    let randomDataFunc: RandomDataFunc

    mutating func nextUInt32() -> UInt32 {
        let bytes = randomDataFunc(4)
        var value: UInt32 = 0
        for (index, byte) in bytes.prefix(4).enumerated() {
            value |= UInt32(byte) << (8 * index)
        }
        return value
    }

    mutating func nextUInt64() -> UInt64 {
        let bytes = randomDataFunc(8)
        var value: UInt64 = 0
        for (index, byte) in bytes.prefix(8).enumerated() {
            value |= UInt64(byte) << (8 * index)
        }
        return value
    }

    mutating func randomData(count: Int) -> Data {
        randomDataFunc(count)
    }

    mutating func fillRandomData(_ data: inout Data) {
        data = randomDataFunc(data.count)
    }
}

public extension Envelope {
    /// Returns a new ``Envelope`` with a `sskrShare: SSKRShare` assertion added.
    func addSSKRShare(_ share: SSKRShare) -> Envelope {
        try! addAssertion(Envelope(.sskrShare, share))
    }
    
    /// Splits the envelope into a set of SSKR shares.
    ///
    /// The envelope subject should already be encrypted by a specific `SymmetricKey`
    /// known as the `contentKey`.
    ///
    /// Each returned envelope will have an `sskrShare: SSKRShare` assertion added to
    /// it.
    ///
    /// - Parameters:
    ///   - groupThreshold: The SSKR group threshold.
    ///   - groups: The number of SSKR groups.
    ///   - contentKey: The `SymmetricKey` used to encrypt the envelope's subject.
    ///
    /// - Returns: An array of arrays. Each element of the outer array represents an
    /// SSKR group, and the elements of each inner array are the envelope with a unique
    /// `sskrShare: SSKRShare` assertion added to each.
    func split(groupThreshold: Int, groups: [(Int, Int)], contentKey: SymmetricKey, testRNG: @escaping RandomDataFunc = secureRandomData) -> [[Envelope]] {
        var rng = ClosureRandomNumberGenerator(randomDataFunc: testRNG)
        return split(groupThreshold: groupThreshold, groups: groups, contentKey: contentKey, using: &rng)
    }

    func split<R: BCRandomNumberGenerator>(groupThreshold: Int, groups: [(Int, Int)], contentKey: SymmetricKey, using rng: inout R) -> [[Envelope]] {
        let groupSpecs = groups.map { group in
            try! SSKRGroupSpec(memberThreshold: group.0, memberCount: group.1)
        }
        let spec = try! SSKRSpec(groupThreshold: groupThreshold, groups: groupSpecs)
        let secret = try! SSKRSecret(Array(contentKey.data))
        let shares = try! sskrGenerateUsing(spec: spec, masterSecret: secret, rng: &rng)
        return shares.map { groupShares in
            groupShares.map { share in
                self.addSSKRShare(share)
            }
        }
    }
    
    /// Creates a new envelope resulting from the joining a set of envelopes split by SSKR.
    ///
    /// Given a set of envelopes that are ostensibly all part of the same SSKR split,
    /// this method attempts to reconstuct the original envelope subject. It will try
    /// all present `sskrShare: SSKRShare` assertions, grouped by split ID, to achieve a
    /// threshold of shares. If it can do so successfully the initializer succeeeds.
    ///
    /// - Parameter envelopes: The envelopes to be joined.
    ///
    /// - Throws: Throws an exception if no quorum of shares can be found to reconstruct
    /// the original envelope.
    init(shares envelopes: [Envelope]) throws {
        guard !envelopes.isEmpty else {
            throw EnvelopeError.invalidShares
        }
        for shares in try Self.shares(in: envelopes).values {
            guard
                let secret = try? sskrCombine(shares),
                let contentKey = try? SymmetricKey(Data(secret.data))
            else {
                continue
            }
            self = try envelopes.first!.decryptSubject(with: contentKey).subject
            return
        }
        throw EnvelopeError.invalidShares
    }
}

extension Envelope {
    /// Returns all the SSKRShares associated with all the `sskrShare: SSKRShare`
    /// assertions of all the given envelopes.
    ///
    /// - Parameter envelopes: The envelopes from which to extract `SSKRShare`s.
    ///
    /// - Returns: A dictionary with share IDs as the keys and an array of `SSKRShare`
    /// as each ID's value. Shares with different IDs are not part of the same SSKR
    /// split.
    static func shares(in envelopes: [Envelope]) throws -> [UInt16: [SSKRShare]] {
        var result: [UInt16: [SSKRShare]] = [:]
        for envelope in envelopes {
            try envelope.assertions(withPredicate: .sskrShare)
                .forEach {
                    let share = try $0.object!.extractSubject(SSKRShare.self)
                    let identifier = share.identifier
                    if result[identifier] == nil {
                        result[identifier] = []
                    }
                    result[identifier]!.append(share)
                }
        }
        return result
    }
}
