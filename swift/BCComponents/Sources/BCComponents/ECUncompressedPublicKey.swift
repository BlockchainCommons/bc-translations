import BCCrypto
import BCTags
import BCUR
import DCBOR
import Foundation

public let ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE = ecdsaUncompressedPublicKeySize

public struct ECUncompressedPublicKey: Equatable, Hashable, Sendable {
    public static let keySize = ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(
            value,
            expected: Self.keySize,
            name: "ECDSA uncompressed public key"
        )
        self.value = value
    }

    public static func fromData(_ data: Data) throws(BCComponentsError) -> ECUncompressedPublicKey {
        try ECUncompressedPublicKey(data)
    }

    public static func fromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> ECUncompressedPublicKey {
        try ECUncompressedPublicKey(Data(data))
    }

    public var data: Data {
        value
    }
}

extension ECUncompressedPublicKey: ECKeyBase {}

extension ECUncompressedPublicKey: ECKey {
    public func publicKey() -> ECPublicKey {
        try! ECPublicKey(ecdsaCompressPublicKey(value))
    }
}

extension ECUncompressedPublicKey: ECPublicKeyBase {
    public func uncompressedPublicKey() -> ECUncompressedPublicKey {
        self
    }
}

extension ECUncompressedPublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.ecKey, .ecKeyV1]
    }

    public var untaggedCBOR: CBOR {
        var map = Map()
        map.insert(3, value)
        return .map(map)
    }
}

extension ECUncompressedPublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .map(let map) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        guard let keyCBOR = map.get(3) else {
            throw BCComponentsError.invalidData(
                dataType: "ECDSA uncompressed public key",
                reason: "missing key data"
            )
        }
        try self.init(byteString(keyCBOR))
    }
}

extension ECUncompressedPublicKey: URCodable {}

extension ECUncompressedPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension ECUncompressedPublicKey: CustomStringConvertible {
    public var description: String {
        "ECUncompressedPublicKey(\(refHexShort()))"
    }
}

extension ECUncompressedPublicKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "ECUncompressedPublicKey(\(hex()))"
    }
}
