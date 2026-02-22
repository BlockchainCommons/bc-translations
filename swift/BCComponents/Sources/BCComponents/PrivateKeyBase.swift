import BCRand
import BCTags
import DCBOR
import Foundation

public struct PrivateKeyBase: Equatable, Hashable, Sendable {
    private let keyData: Data

    public init() {
        var rng = SecureRandomNumberGenerator()
        self = PrivateKeyBase.newUsing(rng: &rng)
    }

    public init(_ data: some DataProtocol) {
        self.keyData = Data(data)
    }

    public static func fromOptionalData(
        _ data: (some DataProtocol)?
    ) -> PrivateKeyBase {
        if let data {
            return PrivateKeyBase(data)
        }
        return PrivateKeyBase()
    }

    public static func newUsing<G: BCRandomNumberGenerator>(
        rng: inout G
    ) -> PrivateKeyBase {
        PrivateKeyBase(rngRandomData(&rng, count: 32))
    }

    public static func newWithProvider(
        _ provider: some PrivateKeyDataProvider
    ) -> PrivateKeyBase {
        PrivateKeyBase(provider.privateKeyData())
    }

    public func ecdsaSigningPrivateKey() -> SigningPrivateKey {
        .newEcdsa(ECPrivateKey.deriveFromKeyMaterial(keyData))
    }

    public func schnorrSigningPrivateKey() -> SigningPrivateKey {
        .newSchnorr(ECPrivateKey.deriveFromKeyMaterial(keyData))
    }

    public func ed25519SigningPrivateKey() -> SigningPrivateKey {
        .newEd25519(Ed25519PrivateKey.deriveFromKeyMaterial(keyData))
    }

    public func sshSigningPrivateKey(
        _ algorithm: SSHAlgorithm,
        comment: String
    ) throws(BCComponentsError) -> SigningPrivateKey {
        let key: SSHPrivateKey
        switch algorithm {
        case .dsa:
            key = SSHPrivateKey.generateDeterministicDsa(
                keyMaterial: keyData,
                comment: comment
            )
        case .ed25519:
            key = try SSHPrivateKey.generateDeterministicEd25519(
                keyMaterial: keyData,
                comment: comment
            )
        case .ecdsaP256, .ecdsaP384:
            key = try SSHPrivateKey.generate(algorithm: algorithm, comment: comment)
        }
        return .newSSH(key)
    }

    public func x25519PrivateKey() -> X25519PrivateKey {
        X25519PrivateKey.deriveFromKeyMaterial(keyData)
    }

    public func schnorrPrivateKeys() -> PrivateKeys {
        .withKeys(
            schnorrSigningPrivateKey(),
            .x25519(x25519PrivateKey())
        )
    }

    public func schnorrPublicKeys() -> PublicKeys {
        .withKeys(
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
        .withKeys(
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
        return try .withKeys(
            privateKey.publicKey(),
            .x25519(x25519PrivateKey().publicKey())
        )
    }

    public var data: Data {
        keyData
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
    public var encapsulationPrivateKey: EncapsulationPrivateKey {
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
        .bytes(keyData)
    }
}

extension PrivateKeyBase: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        self.init(try byteString(untaggedCBOR))
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
