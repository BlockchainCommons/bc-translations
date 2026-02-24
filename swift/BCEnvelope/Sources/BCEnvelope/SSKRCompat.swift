import Foundation
import BCComponents

public extension Envelope {
    func sskrSplit(
        _ spec: SSKRSpec,
        _ contentKey: SymmetricKey
    ) throws -> [[Envelope]] {
        split(
            groupThreshold: spec.groupThreshold,
            groups: spec.groups.map { ($0.memberThreshold, $0.memberCount) },
            contentKey: contentKey
        )
    }

    func sskrSplitFlattened(
        _ spec: SSKRSpec,
        _ contentKey: SymmetricKey
    ) throws -> [Envelope] {
        try sskrSplit(spec, contentKey).flatMap { $0 }
    }

    func sskrSplitUsing<R: BCRandomNumberGenerator>(
        _ spec: SSKRSpec,
        _ contentKey: SymmetricKey,
        _ testRNG: inout R
    ) throws -> [[Envelope]] {
        split(
            groupThreshold: spec.groupThreshold,
            groups: spec.groups.map { ($0.memberThreshold, $0.memberCount) },
            contentKey: contentKey,
            using: &testRNG
        )
    }

    static func sskrJoin(_ envelopes: [Envelope]) throws -> Envelope {
        try Envelope(shares: envelopes)
    }
}
