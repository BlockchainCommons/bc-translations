import BCCrypto
import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct SymmetricKey: Equatable, Hashable, Sendable {
    public static let symmetricKeySize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.symmetricKeySize, name: "symmetric key")
        self.value = value
    }

    public init() {
        var rng = SecureRandomNumberGenerator()
        self = SymmetricKey.newUsing(rng: &rng)
    }

    public static func newUsing<G: BCRandomNumberGenerator>(rng: inout G) -> SymmetricKey {
        try! SymmetricKey(rng.randomData(count: Self.symmetricKeySize))
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> SymmetricKey {
        try SymmetricKey(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public var hex: String {
        hexEncode(value)
    }

    public func encrypt(
        _ plaintext: some DataProtocol,
        aad: (some DataProtocol)? = nil,
        nonce: Nonce? = nil
    ) -> EncryptedMessage {
        let nonce = nonce ?? Nonce()
        let aadBytes = aad.map { Data($0) } ?? Data()
        let encrypted = aeadChaCha20Poly1305Encrypt(
            Data(plaintext),
            key: value,
            nonce: nonce.data,
            aad: aadBytes
        )
        return EncryptedMessage(
            ciphertext: encrypted.ciphertext,
            aad: aadBytes,
            nonce: nonce,
            auth: try! AuthenticationTag(encrypted.tag)
        )
    }

    public func encryptWithDigest(
        _ plaintext: some DataProtocol,
        digest: Digest,
        nonce: Nonce? = nil
    ) -> EncryptedMessage {
        encrypt(plaintext, aad: digest.taggedCBOR.cborData, nonce: nonce)
    }

    public func decrypt(_ message: EncryptedMessage) throws(BCComponentsError) -> Data {
        do {
            return try aeadChaCha20Poly1305Decrypt(
                message.ciphertext,
                key: value,
                nonce: message.nonce.data,
                aad: message.aad,
                tag: message.authenticationTag.data
            )
        } catch {
            throw .crypto(error.localizedDescription)
        }
    }
}

extension SymmetricKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension SymmetricKey: CustomStringConvertible {
    public var description: String {
        "SymmetricKey(\(refHexShort()))"
    }
}

extension SymmetricKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "SymmetricKey(\(hex))"
    }
}

extension SymmetricKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.symmetricKey]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension SymmetricKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension SymmetricKey: URCodable {}
