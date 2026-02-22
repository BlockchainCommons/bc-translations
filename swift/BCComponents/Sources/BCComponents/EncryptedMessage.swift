import BCUR
import BCTags
import DCBOR
import Foundation

public struct EncryptedMessage: Equatable, Sendable {
    private let ciphertextBytes: Data
    private let aadBytes: Data
    private let nonceValue: Nonce
    private let authValue: AuthenticationTag

    public init(
        ciphertext: some DataProtocol,
        aad: some DataProtocol = Data(),
        nonce: Nonce,
        auth: AuthenticationTag
    ) {
        self.ciphertextBytes = Data(ciphertext)
        self.aadBytes = Data(aad)
        self.nonceValue = nonce
        self.authValue = auth
    }

    public func ciphertext() -> Data {
        ciphertextBytes
    }

    public func aad() -> Data {
        aadBytes
    }

    public func nonce() -> Nonce {
        nonceValue
    }

    public func authenticationTag() -> AuthenticationTag {
        authValue
    }

    public func aadCBOR() -> CBOR? {
        if aadBytes.isEmpty {
            return nil
        }
        return try? CBOR(aadBytes)
    }

    public func aadDigest() -> Digest? {
        guard let cbor = aadCBOR() else {
            return nil
        }
        return try? Digest(cbor: cbor)
    }

    public func hasDigest() -> Bool {
        aadDigest() != nil
    }
}

extension EncryptedMessage: DigestProvider {
    public func digest() -> Digest {
        aadDigest()!
    }
}

extension EncryptedMessage: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.encrypted]
    }

    public var untaggedCBOR: CBOR {
        var elements: [CBOR] = [
            .bytes(ciphertextBytes),
            .bytes(nonceValue.data),
            .bytes(authValue.data),
        ]
        if !aadBytes.isEmpty {
            elements.append(.bytes(aadBytes))
        }
        return .array(elements)
    }
}

extension EncryptedMessage: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        if elements.count < 3 {
            throw BCComponentsError.invalidData(dataType: "EncryptedMessage", reason: "must have at least 3 elements")
        }

        let ciphertext = try byteString(elements[0])
        let nonce = try Nonce(try byteString(elements[1]))
        let auth = try AuthenticationTag(try byteString(elements[2]))
        let aad: Data
        if elements.count > 3 {
            aad = try byteString(elements[3])
        } else {
            aad = Data()
        }

        self.init(ciphertext: ciphertext, aad: aad, nonce: nonce, auth: auth)
    }
}

extension EncryptedMessage: URCodable {}
