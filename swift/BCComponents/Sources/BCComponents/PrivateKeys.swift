import BCTags
import DCBOR
import Foundation

public struct PrivateKeys: Equatable, Hashable, Sendable {
    private let signingPrivateKeyValue: SigningPrivateKey
    private let encapsulationPrivateKeyValue: EncapsulationPrivateKey

    public static func withKeys(
        _ signingPrivateKey: SigningPrivateKey,
        _ encapsulationPrivateKey: EncapsulationPrivateKey
    ) -> PrivateKeys {
        PrivateKeys(
            signingPrivateKeyValue: signingPrivateKey,
            encapsulationPrivateKeyValue: encapsulationPrivateKey
        )
    }

    public var signingPrivateKey: SigningPrivateKey {
        signingPrivateKeyValue
    }

    public var encapsulationPrivateKey: EncapsulationPrivateKey {
        encapsulationPrivateKeyValue
    }

    public func publicKeys() throws(BCComponentsError) -> PublicKeys {
        try PublicKeys.withKeys(
            signingPrivateKeyValue.publicKey(),
            encapsulationPrivateKeyValue.publicKey()
        )
    }
}

public protocol PrivateKeysProvider {
    func privateKeys() -> PrivateKeys
}

extension PrivateKeys: PrivateKeysProvider {
    public func privateKeys() -> PrivateKeys {
        self
    }
}

extension PrivateKeys: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension PrivateKeys: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.privateKeys]
    }

    public var untaggedCBOR: CBOR {
        .array([
            signingPrivateKeyValue.cbor,
            encapsulationPrivateKeyValue.cbor,
        ])
    }
}

extension PrivateKeys: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "PrivateKeys",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "PrivateKeys",
                reason: "must have two elements"
            )
        }

        let signingPrivateKey = try SigningPrivateKey(cbor: elements[0])
        let encapsulationPrivateKey = try EncapsulationPrivateKey(cbor: elements[1])
        self = .withKeys(signingPrivateKey, encapsulationPrivateKey)
    }
}

extension PrivateKeys: URCodable {}

extension PrivateKeys: Signer {
    public func signWithOptions(
        _ message: some DataProtocol,
        options: SigningOptions?
    ) throws(BCComponentsError) -> Signature {
        try signingPrivateKeyValue.signWithOptions(message, options: options)
    }
}

extension PrivateKeys: Decrypter {
    // encapsulationPrivateKey property is already defined above
}

extension PrivateKeys: CustomStringConvertible {
    public var description: String {
        "PrivateKeys(\(refHexShort()), \(signingPrivateKeyValue), \(encapsulationPrivateKeyValue))"
    }
}
