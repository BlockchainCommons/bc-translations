import DCBOR
import Foundation
import SwiftKyber

public enum MLKEM: UInt32, Equatable, Hashable, Sendable {
    case mlkem512 = 512
    case mlkem768 = 768
    case mlkem1024 = 1024

    public static let sharedSecretSize = 32

    public func keypair() -> (MLKEMPrivateKey, MLKEMPublicKey) {
        let generated = Kyber.GenerateKeyPair(kind: kyberKind)
        let privateKey = try! MLKEMPrivateKey.fromBytes(
            self,
            Data(generated.decap.keyBytes)
        )
        let publicKey = try! MLKEMPublicKey.fromBytes(
            self,
            Data(generated.encap.keyBytes)
        )
        return (privateKey, publicKey)
    }

    public func privateKeySize() -> Int {
        switch self {
        case .mlkem512:
            return 1632
        case .mlkem768:
            return 2400
        case .mlkem1024:
            return 3168
        }
    }

    public func publicKeySize() -> Int {
        switch self {
        case .mlkem512:
            return 800
        case .mlkem768:
            return 1184
        case .mlkem1024:
            return 1568
        }
    }

    public func sharedSecretSize() -> Int {
        Self.sharedSecretSize
    }

    public func ciphertextSize() -> Int {
        switch self {
        case .mlkem512:
            return 768
        case .mlkem768:
            return 1088
        case .mlkem1024:
            return 1568
        }
    }

    var kyberKind: SwiftKyber.Kind {
        switch self {
        case .mlkem512:
            return .K512
        case .mlkem768:
            return .K768
        case .mlkem1024:
            return .K1024
        }
    }
}

extension MLKEM: CBOREncodable {
    public var cbor: CBOR {
        .unsigned(UInt64(rawValue))
    }
}

extension MLKEM: CBORDecodable {
    public init(cbor: CBOR) throws {
        let value = try UInt32(cbor: cbor)
        guard let level = MLKEM(rawValue: value) else {
            throw BCComponentsError.postQuantum("Invalid MLKEM level: \(value)")
        }
        self = level
    }
}
