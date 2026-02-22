import BCCrypto
import BCTags
import BCRand
import BCUR
import DCBOR
import Foundation

public let ECDSA_PRIVATE_KEY_SIZE = ecdsaPrivateKeySize

public struct ECPrivateKey: Equatable, Hashable, Sendable {
    public static let keySize = ECDSA_PRIVATE_KEY_SIZE

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "EC private key")
        self.value = value
    }

    public static func new() -> ECPrivateKey {
        var rng = SecureRandomNumberGenerator()
        return newUsing(rng: &rng)
    }

    public static func newUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> ECPrivateKey {
        try! ECPrivateKey(ecdsaNewPrivateKeyUsing(&rng))
    }

    public static func fromData(_ data: Data) throws(BCComponentsError) -> ECPrivateKey {
        try ECPrivateKey(data)
    }

    public static func fromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> ECPrivateKey {
        try ECPrivateKey(Data(data))
    }

    public static func deriveFromKeyMaterial(
        _ keyMaterial: some DataProtocol
    ) -> ECPrivateKey {
        try! ECPrivateKey(ecdsaDerivePrivateKey(Data(keyMaterial)))
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
        value
    }

    public func schnorrPublicKey() -> SchnorrPublicKey {
        try! SchnorrPublicKey(schnorrPublicKeyFromPrivateKey(value))
    }

    public func ecdsaSign(_ message: some DataProtocol) -> Data {
        BCCrypto.ecdsaSign(value, Data(message))
    }

    public func schnorrSignUsing<G: BCRandomNumberGenerator>(
        _ message: some DataProtocol,
        rng: inout G
    ) -> Data {
        BCCrypto.schnorrSignUsing(value, Data(message), &rng)
    }

    public func schnorrSign(_ message: some DataProtocol) -> Data {
        BCCrypto.schnorrSign(value, Data(message))
    }
}

extension ECPrivateKey: ECKeyBase {}

extension ECPrivateKey: ECKey {
    public func publicKey() -> ECPublicKey {
        try! ECPublicKey(ecdsaPublicKeyFromPrivateKey(value))
    }
}

extension ECPrivateKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.ecKey, .ecKeyV1]
    }

    public var untaggedCBOR: CBOR {
        var map = Map()
        map.insert(2, true)
        map.insert(3, value)
        return .map(map)
    }
}

extension ECPrivateKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .map(let map) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        guard
            let privateFlagCBOR = map.get(2),
            let privateFlag = try? Bool(cbor: privateFlagCBOR),
            privateFlag
        else {
            throw BCComponentsError.invalidData(
                dataType: "EC private key",
                reason: "missing private key marker"
            )
        }
        guard let keyCBOR = map.get(3) else {
            throw BCComponentsError.invalidData(
                dataType: "EC private key",
                reason: "missing key data"
            )
        }
        try self.init(byteString(keyCBOR))
    }
}

extension ECPrivateKey: URCodable {}

extension ECPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension ECPrivateKey: CustomStringConvertible {
    public var description: String {
        "ECPrivateKey(\(refHexShort()))"
    }
}

extension ECPrivateKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "ECPrivateKey(\(hex()))"
    }
}
