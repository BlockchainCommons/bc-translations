import BCCrypto
import Foundation

public let SCHNORR_PUBLIC_KEY_SIZE = schnorrPublicKeySize

public struct SchnorrPublicKey: Equatable, Hashable, Sendable {
    public static let keySize = SCHNORR_PUBLIC_KEY_SIZE

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "Schnorr public key")
        self.value = value
    }

    public static func fromData(_ data: Data) throws(BCComponentsError) -> SchnorrPublicKey {
        try SchnorrPublicKey(data)
    }

    public static func fromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> SchnorrPublicKey {
        try SchnorrPublicKey(Data(data))
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
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
        "SchnorrPublicKey(\(hex()))"
    }
}
