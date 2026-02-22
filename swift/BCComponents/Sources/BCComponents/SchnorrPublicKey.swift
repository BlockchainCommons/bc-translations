import BCCrypto
import Foundation

public struct SchnorrPublicKey: Equatable, Hashable, Sendable {
    public static let keySize = schnorrPublicKeySize

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "Schnorr public key")
        self.value = value
    }

    public var data: Data {
        value
    }

    public func schnorrVerify(
        _ signature: Data,
        _ message: some DataProtocol
    ) -> Bool {
        BCCrypto.schnorrVerify(value, signature, Data(message))
    }
}

extension SchnorrPublicKey: ECKeyBase {}

extension SchnorrPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(value))
    }
}

extension SchnorrPublicKey: CustomStringConvertible {
    public var description: String {
        "SchnorrPublicKey(\(refHexShort()))"
    }
}

extension SchnorrPublicKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "SchnorrPublicKey(\(hex))"
    }
}
