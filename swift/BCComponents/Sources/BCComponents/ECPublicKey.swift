import BCCrypto
import BCTags
import BCUR
import DCBOR
import Foundation

public let ECDSA_PUBLIC_KEY_SIZE = ecdsaPublicKeySize

public struct ECPublicKey: Equatable, Hashable, Sendable {
    public static let keySize = ECDSA_PUBLIC_KEY_SIZE

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "ECDSA public key")
        self.value = value
    }

    public static func fromData(_ data: Data) throws(BCComponentsError) -> ECPublicKey {
        try ECPublicKey(data)
    }

    public static func fromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> ECPublicKey {
        try ECPublicKey(Data(data))
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
        value
    }

    public func verify(
        _ signature: Data,
        _ message: some DataProtocol
    ) -> Bool {
        ecdsaVerify(value, signature, Data(message))
    }
}

extension ECPublicKey: ECKeyBase {}

extension ECPublicKey: ECKey {
    public func publicKey() -> ECPublicKey {
        self
    }
}

extension ECPublicKey: ECPublicKeyBase {
    public func uncompressedPublicKey() -> ECUncompressedPublicKey {
        try! ECUncompressedPublicKey(ecdsaDecompressPublicKey(value))
    }
}

extension ECPublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.ecKey, .ecKeyV1]
    }

    public var untaggedCBOR: CBOR {
        var map = Map()
        map.insert(3, value)
        return .map(map)
    }
}

extension ECPublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .map(let map) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        guard let keyCBOR = map.get(3) else {
            throw BCComponentsError.invalidData(
                dataType: "ECDSA public key",
                reason: "missing key data"
            )
        }
        try self.init(byteString(keyCBOR))
    }
}

extension ECPublicKey: URCodable {}

extension ECPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension ECPublicKey: CustomStringConvertible {
    public var description: String {
        "ECPublicKey(\(refHexShort()))"
    }
}

extension ECPublicKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "ECPublicKey(\(hex()))"
    }
}
