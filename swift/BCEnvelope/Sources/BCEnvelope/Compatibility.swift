import BCComponents
import BCTags
import DCBOR
import Foundation
import KnownValues

// Compatibility helpers for adapting the legacy Swift envelope surface to
// current in-repo BCComponents/KnownValues APIs.

public extension DigestProvider {
    var digest: Digest { digest() }
}

public extension Swift.Set where Element == Digest {
    mutating func insert(_ digestProvider: some DigestProvider) {
        insert(digestProvider.digest())
    }

    mutating func insert(_ digests: Swift.Set<Digest>) {
        formUnion(digests)
    }
}

public let globalKnownValues: KnownValuesStore = KnownValuesStore.shared

public extension KnownValue {
    // Convenience aliases for backward-compatible test code.
    static let OK = KnownValue.okValue
    static let unknown = KnownValue.unknownValue
    static let Seed = KnownValue.seedType
}

public extension KnownValuesStore {
    static func knownValue(
        for rawValue: UInt64,
        knownValues: KnownValuesStore? = nil
    ) -> KnownValue {
        knownValue(forRawValue: rawValue, in: knownValues)
    }

    static func name(
        for knownValue: KnownValue,
        knownValues: KnownValuesStore? = nil
    ) -> String {
        name(for: knownValue, in: knownValues)
    }
}

public extension Digest {
    init(_ image: String) {
        self = Digest.fromImage(Data(image.utf8))
    }

    init?(rawValue: some DataProtocol) {
        guard let digest = try? Digest(Data(rawValue)) else {
            return nil
        }
        self = digest
    }
}

public typealias PublicKeyBase = PublicKeys

public extension PrivateKeyBase {
    init(_ provider: some PrivateKeyDataProvider) {
        self = .newWithProvider(provider)
    }

    init(testKeyMaterial: some DataProtocol) {
        self = .newWithProvider(Data(testKeyMaterial))
    }

    var signingPrivateKey: SigningPrivateKey {
        schnorrSigningPrivateKey()
    }

    var signingPublicKey: SigningPublicKey {
        schnorrPublicKeys().signingPublicKey
    }

    var privateKeys: PrivateKeys {
        privateKeys()
    }

    var publicKeys: PublicKeys {
        publicKeys()
    }
}

public extension SigningPrivateKey {
    func secp256k1SchnorrSign<R: BCRandomNumberGenerator>(
        _ digest: Digest,
        using rng: inout R
    ) throws(BCComponentsError) -> Signature {
        guard let key = toSchnorr() else {
            throw BCComponentsError.crypto("Invalid key type for Schnorr signing")
        }
        return try .schnorrFromData(key.schnorrSignUsing(digest.data, rng: &rng))
    }
}

public extension SigningPublicKey {
    func verify(signature: Signature, for digest: Digest) -> Bool {
        verify(signature, digest.data)
    }
}

public extension SymmetricKey {
    init(_ hex: String) {
        self = try! .fromHex(hex)
    }

    func encrypt(
        plaintext: some DataProtocol,
        digest: Digest,
        nonce: Nonce? = nil
    ) -> EncryptedMessage {
        encryptWithDigest(plaintext, digest: digest, nonce: nonce)
    }

    func decrypt(message: EncryptedMessage) throws(BCComponentsError) -> Data {
        try decrypt(message)
    }
}

@MainActor private var knownTagsAdded = false

@MainActor
public func addKnownTags() async {
    guard !knownTagsAdded else {
        return
    }

    BCTags.registerTags()
    knownTagsAdded = true
}
