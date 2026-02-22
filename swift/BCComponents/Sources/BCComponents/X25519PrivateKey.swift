import BCCrypto
import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct X25519PrivateKey: Equatable, Hashable, Sendable {
    public static let keySize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "X25519 private key")
        self.value = value
    }

    public init() {
        var rng = SecureRandomNumberGenerator()
        self = X25519PrivateKey.newUsing(rng: &rng)
    }

    public static func keypair() -> (X25519PrivateKey, X25519PublicKey) {
        let privateKey = X25519PrivateKey()
        return (privateKey, privateKey.publicKey())
    }

    public static func keypairUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> (X25519PrivateKey, X25519PublicKey) {
        let privateKey = newUsing(rng: &rng)
        return (privateKey, privateKey.publicKey())
    }

    public static func newUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> X25519PrivateKey {
        try! X25519PrivateKey(x25519NewPrivateKeyUsing(&rng))
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> X25519PrivateKey {
        try X25519PrivateKey(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public var hex: String {
        hexEncode(value)
    }

    public func publicKey() -> X25519PublicKey {
        try! X25519PublicKey(x25519PublicKeyFromPrivateKey(value))
    }

    public static func deriveFromKeyMaterial(_ keyMaterial: some DataProtocol) -> X25519PrivateKey {
        try! X25519PrivateKey(deriveAgreementPrivateKey(Data(keyMaterial)))
    }

    public func sharedKey(with publicKey: X25519PublicKey) -> SymmetricKey {
        try! SymmetricKey(x25519SharedKey(privateKey: value, publicKey: publicKey.data))
    }
}

extension X25519PrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension X25519PrivateKey: CustomStringConvertible {
    public var description: String {
        "X25519PrivateKey(\(refHexShort()))"
    }
}

extension X25519PrivateKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "X25519PrivateKey(\(hex))"
    }
}

extension X25519PrivateKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.x25519PrivateKey]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension X25519PrivateKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension X25519PrivateKey: URCodable {}
