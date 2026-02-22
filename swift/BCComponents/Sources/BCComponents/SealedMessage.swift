import BCUR
import BCTags
import DCBOR
import Foundation

public struct SealedMessage: Equatable, Sendable {
    private let message: EncryptedMessage
    private let encapsulatedKey: EncapsulationCiphertext

    public init(
        message: EncryptedMessage,
        encapsulatedKey: EncapsulationCiphertext
    ) {
        self.message = message
        self.encapsulatedKey = encapsulatedKey
    }

    public static func new(
        _ plaintext: some DataProtocol,
        recipient: any Encrypter
    ) -> SealedMessage {
        newWithAAD(plaintext, recipient: recipient, aad: Data?.none)
    }

    public static func newWithAAD(
        _ plaintext: some DataProtocol,
        recipient: any Encrypter,
        aad: (some DataProtocol)?
    ) -> SealedMessage {
        newOpt(plaintext, recipient: recipient, aad: aad, testNonce: nil)
    }

    public static func newOpt(
        _ plaintext: some DataProtocol,
        recipient: any Encrypter,
        aad: (some DataProtocol)?,
        testNonce: Nonce?
    ) -> SealedMessage {
        let (sharedKey, encapsulatedKey) = recipient.encapsulateNewSharedSecret()
        let encryptedMessage = sharedKey.encrypt(
            plaintext,
            aad: aad,
            nonce: testNonce
        )
        return SealedMessage(
            message: encryptedMessage,
            encapsulatedKey: encapsulatedKey
        )
    }

    public func decrypt(
        _ privateKey: any Decrypter
    ) throws(BCComponentsError) -> Data {
        let sharedKey = try privateKey.decapsulateSharedSecret(encapsulatedKey)
        return try sharedKey.decrypt(message)
    }

    public var encapsulationScheme: EncapsulationScheme {
        encapsulatedKey.encapsulationScheme
    }
}

extension SealedMessage: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.sealedMessage]
    }

    public var untaggedCBOR: CBOR {
        .array([
            message.cbor,
            encapsulatedKey.cbor,
        ])
    }
}

extension SealedMessage: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "SealedMessage",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "SealedMessage",
                reason: "must have two elements"
            )
        }

        let message = try EncryptedMessage(cbor: elements[0])
        let encapsulatedKey = try EncapsulationCiphertext(cbor: elements[1])
        self.init(message: message, encapsulatedKey: encapsulatedKey)
    }
}

extension SealedMessage: URCodable {}
