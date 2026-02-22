import BCCrypto
import BCUR
import BCTags
import DCBOR
import Foundation

public struct Digest: Equatable, Hashable, Sendable {
    public static let digestSize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.digestSize, name: "digest")
        self.value = value
    }

    public static func fromImage(_ image: some DataProtocol) -> Digest {
        try! Digest(sha256(Data(image)))
    }

    public static func fromImageParts(_ imageParts: [Data]) -> Digest {
        var data = Data()
        for part in imageParts {
            data.append(part)
        }
        return fromImage(data)
    }

    public static func fromDigests(_ digests: [Digest]) -> Digest {
        var data = Data()
        for digest in digests {
            data.append(digest.data)
        }
        return fromImage(data)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Digest {
        try Digest(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public func validate(_ image: some DataProtocol) -> Bool {
        self == Digest.fromImage(image)
    }

    public var hex: String {
        hexEncode(value)
    }

    public var shortDescription: String {
        hexEncode(value.prefix(4))
    }

    public static func validateOpt(
        image: some DataProtocol,
        digest: Digest?
    ) -> Bool {
        if let digest {
            return digest.validate(image)
        }
        return true
    }
}

extension Digest: DigestProvider {
    public func digest() -> Digest {
        self
    }
}

extension Digest: CustomStringConvertible {
    public var description: String {
        "Digest(\(hex))"
    }
}

extension Digest: CustomDebugStringConvertible {
    public var debugDescription: String {
        description
    }
}

extension Digest: Comparable {
    public static func < (lhs: Digest, rhs: Digest) -> Bool {
        lhs.value.lexicographicallyPrecedes(rhs.value)
    }
}

extension Digest: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.digest]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension Digest: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension Digest: URCodable {}
