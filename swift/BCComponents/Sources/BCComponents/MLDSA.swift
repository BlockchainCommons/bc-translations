import DCBOR
import Foundation
import SwiftDilithium

public enum MLDSA: UInt32, Equatable, Hashable, Sendable {
    case mldsa44 = 2
    case mldsa65 = 3
    case mldsa87 = 5

    public func keypair() -> (MLDSAPrivateKey, MLDSAPublicKey) {
        let generated = Dilithium.GenerateKeyPair(kind: dilithiumKind)
        let privateKey = try! MLDSAPrivateKey.fromBytes(
            self,
            Data(generated.sk.keyBytes)
        )
        let publicKey = try! MLDSAPublicKey.fromBytes(
            self,
            Data(generated.pk.keyBytes)
        )
        return (privateKey, publicKey)
    }

    public func privateKeySize() -> Int {
        switch self {
        case .mldsa44:
            return 2560
        case .mldsa65:
            return 4032
        case .mldsa87:
            return 4896
        }
    }

    public func publicKeySize() -> Int {
        switch self {
        case .mldsa44:
            return 1312
        case .mldsa65:
            return 1952
        case .mldsa87:
            return 2592
        }
    }

    public func signatureSize() -> Int {
        switch self {
        case .mldsa44:
            return 2420
        case .mldsa65:
            return 3309
        case .mldsa87:
            return 4627
        }
    }

    var dilithiumKind: SwiftDilithium.Kind {
        switch self {
        case .mldsa44:
            return .ML_DSA_44
        case .mldsa65:
            return .ML_DSA_65
        case .mldsa87:
            return .ML_DSA_87
        }
    }
}

extension MLDSA: CBOREncodable {
    public var cbor: CBOR {
        .unsigned(UInt64(rawValue))
    }
}

extension MLDSA: CBORDecodable {
    public init(cbor: CBOR) throws {
        let value = try UInt32(cbor: cbor)
        guard let level = MLDSA(rawValue: value) else {
            throw BCComponentsError.postQuantum("Invalid MLDSA level: \(value)")
        }
        self = level
    }
}
