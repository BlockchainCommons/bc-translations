import BCCrypto
import BCRand
import Foundation

public let ED25519_PRIVATE_KEY_SIZE = ed25519PrivateKeySize

public struct Ed25519PrivateKey: Equatable, Hashable, Sendable {
    public static let keySize = ED25519_PRIVATE_KEY_SIZE

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "Ed25519 private key")
        self.value = value
    }

    public static func new() -> Ed25519PrivateKey {
        var rng = SecureRandomNumberGenerator()
        return newUsing(rng: &rng)
    }

    public static func newUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> Ed25519PrivateKey {
        try! Ed25519PrivateKey(ed25519NewPrivateKeyUsing(&rng))
    }

    public static func fromData(_ data: Data) throws(BCComponentsError) -> Ed25519PrivateKey {
        try Ed25519PrivateKey(data)
    }

    public static func fromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> Ed25519PrivateKey {
        try Ed25519PrivateKey(Data(data))
    }

    public static func deriveFromKeyMaterial(
        _ keyMaterial: some DataProtocol
    ) -> Ed25519PrivateKey {
        try! Ed25519PrivateKey(deriveSigningPrivateKey(Data(keyMaterial)))
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
        value
    }

    public func hex() -> String {
        hexEncode(value)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Ed25519PrivateKey {
        try Ed25519PrivateKey(parseHex(hex))
    }

    public func publicKey() -> Ed25519PublicKey {
        try! Ed25519PublicKey(ed25519PublicKeyFromPrivateKey(value))
    }

    public func sign(_ message: some DataProtocol) -> Data {
        ed25519Sign(value, Data(message))
    }
}

extension Ed25519PrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(value))
    }
}

extension Ed25519PrivateKey: CustomStringConvertible {
    public var description: String {
        "Ed25519PrivateKey(\(refHexShort()))"
    }
}

extension Ed25519PrivateKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "Ed25519PrivateKey(\(hex()))"
    }
}
