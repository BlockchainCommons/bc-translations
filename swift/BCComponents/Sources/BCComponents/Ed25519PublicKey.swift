import BCCrypto
import Foundation

public struct Ed25519PublicKey: Equatable, Hashable, Sendable {
    public static let keySize = ed25519PublicKeySize

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "Ed25519 public key")
        self.value = value
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Ed25519PublicKey {
        try Ed25519PublicKey(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public func verify(
        _ signature: Data,
        _ message: some DataProtocol
    ) -> Bool {
        ed25519Verify(value, Data(message), signature)
    }
}

extension Ed25519PublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(value))
    }
}

extension Ed25519PublicKey: CustomStringConvertible {
    public var description: String {
        "Ed25519PublicKey(\(refHexShort()))"
    }
}

extension Ed25519PublicKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "Ed25519PublicKey(\(hexEncode(value)))"
    }
}
