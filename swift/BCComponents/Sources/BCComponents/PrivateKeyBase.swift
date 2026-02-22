import BCRand
import BCTags
import DCBOR
import Foundation

public struct PrivateKeyBase: Equatable, Hashable, Sendable {
    private let data: Data

    public static func new() -> PrivateKeyBase {
        var rng = SecureRandomNumberGenerator()
        return newUsing(rng: &rng)
    }

    public static func fromData(_ data: some DataProtocol) -> PrivateKeyBase {
        PrivateKeyBase(data: Data(data))
    }

    public static func fromOptionalData(
        _ data: (some DataProtocol)?
    ) -> PrivateKeyBase {
        if let data {
            return fromData(data)
        }
        return new()
    }

    public static func newUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> PrivateKeyBase {
        PrivateKeyBase(data: rngRandomData(&rng, count: 32))
    }

    public static func newWithProvider(
        _ provider: some PrivateKeyDataProvider
    ) -> PrivateKeyBase {
        fromData(provider.privateKeyData())
    }

    public func ecdsaSigningPrivateKey() -> SigningPrivateKey {
        .newEcdsa(ECPrivateKey.deriveFromKeyMaterial(data))
    }

    public func schnorrSigningPrivateKey() -> SigningPrivateKey {
        .newSchnorr(ECPrivateKey.deriveFromKeyMaterial(data))
    }

    public func ed25519SigningPrivateKey() -> SigningPrivateKey {
        .newEd25519(Ed25519PrivateKey.deriveFromKeyMaterial(data))
    }

    public func sshSigningPrivateKey(
        _ algorithm: SSHAlgorithm,
        comment: String
    ) throws(BCComponentsError) -> SigningPrivateKey {
        let key: SSHPrivateKey
        switch algorithm {
        case .dsa:
            key = SSHPrivateKey.generateDeterministicDsa(
                keyMaterial: data,
                comment: comment
            )
        case .ed25519:
            key = try SSHPrivateKey.generateDeterministicEd25519(
                keyMaterial: data,
                comment: comment
            )
        case .ecdsaP256, .ecdsaP384:
            key = try SSHPrivateKey.generate(algorithm: algorithm, comment: comment)
        }
        return .newSSH(key)
    }

    public func x25519PrivateKey() -> X25519PrivateKey {
        X25519PrivateKey.deriveFromKeyMaterial(data)
    }

    public func schnorrPrivateKeys() -> PrivateKeys {
        .withKeys(
            schnorrSigningPrivateKey(),
            .x25519(x25519PrivateKey())
        )
    }

    public func schnorrPublicKeys() -> PublicKeys {
        .new(
            try! schnorrSigningPrivateKey().publicKey(),
            .x25519(x25519PrivateKey().publicKey())
        )
    }

    public func ecdsaPrivateKeys() -> PrivateKeys {
        .withKeys(
            ecdsaSigningPrivateKey(),
            .x25519(x25519PrivateKey())
        )
    }

    public func ecdsaPublicKeys() -> PublicKeys {
        .new(
            try! ecdsaSigningPrivateKey().publicKey(),
            .x25519(x25519PrivateKey().publicKey())
        )
    }

    public func sshPrivateKeys(
        _ algorithm: SSHAlgorithm,
        comment: String
    ) throws(BCComponentsError) -> PrivateKeys {
        let privateKey = try sshSigningPrivateKey(algorithm, comment: comment)
        return .withKeys(
            privateKey,
            .x25519(x25519PrivateKey())
        )
    }

    public func sshPublicKeys(
        _ algorithm: SSHAlgorithm,
        comment: String
    ) throws(BCComponentsError) -> PublicKeys {
        let privateKey = try sshSigningPrivateKey(algorithm, comment: comment)
        return try .new(
            privateKey.publicKey(),
            .x25519(x25519PrivateKey().publicKey())
        )
    }

    public func asBytes() -> Data {
        data
    }
}

extension PrivateKeyBase: Signer {
    public func signWithOptions(
        _ message: some DataProtocol,
        options: SigningOptions?
    ) throws(BCComponentsError) -> Signature {
        try schnorrSigningPrivateKey().signWithOptions(message, options: options)
    }
}

extension PrivateKeyBase: Verifier {
    public func verify(_ signature: Signature, _ message: some DataProtocol) -> Bool {
        switch signature {
        case .schnorr:
            return schnorrSigningPrivateKey().verify(signature, message)
        case .ecdsa, .ed25519, .ssh, .mldsa:
            return false
        }
    }
}

extension PrivateKeyBase: Decrypter {
    public func encapsulationPrivateKey() -> EncapsulationPrivateKey {
        .x25519(x25519PrivateKey())
    }
}

extension PrivateKeyBase: PrivateKeysProvider {
    public func privateKeys() -> PrivateKeys {
        .withKeys(
            schnorrSigningPrivateKey(),
            .x25519(x25519PrivateKey())
        )
    }
}

extension PrivateKeyBase: PublicKeysProvider {
    public func publicKeys() -> PublicKeys {
        schnorrPublicKeys()
    }
}

extension PrivateKeyBase: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.privateKeyBase]
    }

    public var untaggedCBOR: CBOR {
        .bytes(data)
    }
}

extension PrivateKeyBase: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        self = .fromData(try byteString(untaggedCBOR))
    }
}

extension PrivateKeyBase: URCodable {}

extension PrivateKeyBase: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension PrivateKeyBase: CustomStringConvertible {
    public var description: String {
        "PrivateKeyBase(\(refHexShort()))"
    }
}

extension PrivateKeyBase: CustomDebugStringConvertible {
    public var debugDescription: String {
        "PrivateKeyBase"
    }
}
