import BCTags
import DCBOR
import Foundation

public struct PublicKeys: Equatable, Hashable, Sendable {
    private let signingPublicKeyValue: SigningPublicKey
    private let encapsulationPublicKeyValue: EncapsulationPublicKey

    public static func new(
        _ signingPublicKey: SigningPublicKey,
        _ encapsulationPublicKey: EncapsulationPublicKey
    ) -> PublicKeys {
        PublicKeys(
            signingPublicKeyValue: signingPublicKey,
            encapsulationPublicKeyValue: encapsulationPublicKey
        )
    }

    public func signingPublicKey() -> SigningPublicKey {
        signingPublicKeyValue
    }

    public func encapsulationPublicKey() -> EncapsulationPublicKey {
        encapsulationPublicKeyValue
    }

    public func enapsulationPublicKey() -> EncapsulationPublicKey {
        encapsulationPublicKeyValue
    }
}

public protocol PublicKeysProvider {
    func publicKeys() -> PublicKeys
}

extension PublicKeys: PublicKeysProvider {
    public func publicKeys() -> PublicKeys {
        self
    }
}

extension PublicKeys: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension PublicKeys: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.publicKeys]
    }

    public var untaggedCBOR: CBOR {
        .array([
            signingPublicKeyValue.cbor,
            encapsulationPublicKeyValue.cbor,
        ])
    }
}

extension PublicKeys: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "PublicKeys",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "PublicKeys",
                reason: "must have two elements"
            )
        }

        let signingPublicKey = try SigningPublicKey(cbor: elements[0])
        let encapsulationPublicKey = try EncapsulationPublicKey(cbor: elements[1])
        self = .new(signingPublicKey, encapsulationPublicKey)
    }
}

extension PublicKeys: URCodable {}

extension PublicKeys: Verifier {
    public func verify(_ signature: Signature, _ message: some DataProtocol) -> Bool {
        signingPublicKeyValue.verify(signature, message)
    }
}

extension PublicKeys: Encrypter {
}

extension PublicKeys: CustomStringConvertible {
    public var description: String {
        "PublicKeys(\(refHexShort()), \(signingPublicKeyValue), \(encapsulationPublicKeyValue))"
    }
}
