import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation
import SSKR

public typealias SSKRGroupSpec = GroupSpec
public typealias SSKRSecret = Secret
public typealias SSKRSpec = Spec
public typealias SSKRError = SSKR.SSKRError

public struct SSKRShare: Equatable, Hashable, Sendable {
    private let value: Data

    public init(_ value: Data) {
        self.value = value
    }

    public static func fromData(_ value: Data) -> SSKRShare {
        SSKRShare(value)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> SSKRShare {
        SSKRShare(try parseHex(hex))
    }

    public func asBytes() -> Data {
        value
    }

    public func hex() -> String {
        hexEncode(value)
    }

    public func identifier() -> UInt16 {
        (UInt16(value[0]) << 8) | UInt16(value[1])
    }

    public func identifierHex() -> String {
        hexEncode(value.prefix(2))
    }

    public func groupThreshold() -> Int {
        Int(value[2] >> 4) + 1
    }

    public func groupCount() -> Int {
        Int(value[2] & 0x0f) + 1
    }

    public func groupIndex() -> Int {
        Int(value[3] >> 4)
    }

    public func memberThreshold() -> Int {
        Int(value[3] & 0x0f) + 1
    }

    public func memberIndex() -> Int {
        Int(value[4] & 0x0f)
    }
}

extension SSKRShare: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.sskrShare]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension SSKRShare: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        self.init(try byteString(untaggedCBOR))
    }
}

extension SSKRShare: URCodable {}

public func sskrGenerate(
    spec: SSKRSpec,
    masterSecret: SSKRSecret
) throws(SSKRError) -> [[SSKRShare]] {
    let groups = try sskrGenerate(spec: spec, secret: masterSecret)
    return groups.map { group in
        group.map { share in
            SSKRShare(Data(share))
        }
    }
}

public func sskrGenerateUsing<G: BCRandomNumberGenerator>(
    spec: SSKRSpec,
    masterSecret: SSKRSecret,
    rng: inout G
) throws(SSKRError) -> [[SSKRShare]] {
    let groups = try sskrGenerateUsing(
        spec: spec,
        secret: masterSecret,
        randomGenerator: &rng
    )
    return groups.map { group in
        group.map { share in
            SSKRShare(Data(share))
        }
    }
}

public func sskrCombine(
    _ shares: [SSKRShare]
) throws(SSKRError) -> SSKRSecret {
    try sskrCombine(shares: shares.map { Array($0.asBytes()) })
}
