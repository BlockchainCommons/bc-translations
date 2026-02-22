import BCTags
import DCBOR
import Foundation
import SwiftKyber

public struct MLKEMPrivateKey: Equatable, Hashable, Sendable {
    private let levelValue: MLKEM
    private let keyData: Data

    public static func fromBytes(
        _ level: MLKEM,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLKEMPrivateKey {
        try MLKEMPrivateKey(level: level, bytes: Data(bytes))
    }

    private init(level: MLKEM, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.privateKeySize(),
            name: "MLKEM private key"
        )
        do {
            _ = try DecapsulationKey(keyBytes: Array(bytes))
        } catch {
            throw postQuantumError(error)
        }
        self.levelValue = level
        self.keyData = bytes
    }

    public func level() -> MLKEM {
        levelValue
    }

    public func size() -> Int {
        levelValue.privateKeySize()
    }

    public func asBytes() -> Data {
        keyData
    }

    public func decapsulateSharedSecret(
        _ ciphertext: MLKEMCiphertext
    ) throws(BCComponentsError) -> SymmetricKey {
        guard ciphertext.level() == levelValue else {
            throw BCComponentsError.crypto("MLKEM level mismatch")
        }
        do {
            let key = try DecapsulationKey(keyBytes: Array(keyData))
            let secret = try key.Decapsulate(ct: ciphertext.bytesArray)
            return try SymmetricKey.fromData(Data(secret))
        } catch {
            throw postQuantumError(error)
        }
    }
}

extension MLKEMPrivateKey: Decrypter {
    public func encapsulationPrivateKey() -> EncapsulationPrivateKey {
        .mlkem(self)
    }
}

extension MLKEMPrivateKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mlkemPrivateKey]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(keyData)])
    }
}

extension MLKEMPrivateKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMPrivateKey",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMPrivateKey",
                reason: "must have two elements"
            )
        }
        let level = try MLKEM(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLKEMPrivateKey.fromBytes(level, bytes)
    }
}

extension MLKEMPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension MLKEMPrivateKey: CustomStringConvertible {
    public var description: String {
        switch levelValue {
        case .mlkem512:
            return "MLKEM512PrivateKey(\(refHexShort()))"
        case .mlkem768:
            return "MLKEM768PrivateKey(\(refHexShort()))"
        case .mlkem1024:
            return "MLKEM1024PrivateKey(\(refHexShort()))"
        }
    }
}

private func postQuantumError(_ error: any Swift.Error) -> BCComponentsError {
    if let error = error as? BCComponentsError {
        return error
    }
    return .postQuantum(String(describing: error))
}
