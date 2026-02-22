import BCTags
import DCBOR
import Foundation
import SwiftKyber

public struct MLKEMPublicKey: Equatable, Hashable, Sendable {
    private let levelValue: MLKEM
    private let keyData: Data

    public static func fromBytes(
        _ level: MLKEM,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLKEMPublicKey {
        try MLKEMPublicKey(level: level, bytes: Data(bytes))
    }

    private init(level: MLKEM, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.publicKeySize(),
            name: "MLKEM public key"
        )
        do {
            _ = try EncapsulationKey(keyBytes: Array(bytes))
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
        levelValue.publicKeySize()
    }

    public func asBytes() -> Data {
        keyData
    }

    public func encapsulateNewSharedSecret() -> (SymmetricKey, MLKEMCiphertext) {
        let key = try! EncapsulationKey(keyBytes: Array(keyData))
        let encapsulated = key.Encapsulate()
        let sharedSecret = try! SymmetricKey.fromData(Data(encapsulated.K))
        let ciphertext = try! MLKEMCiphertext.fromBytes(
            levelValue,
            Data(encapsulated.ct)
        )
        return (sharedSecret, ciphertext)
    }
}

extension MLKEMPublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mlkemPublicKey]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(keyData)])
    }
}

extension MLKEMPublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMPublicKey",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMPublicKey",
                reason: "must have two elements"
            )
        }
        let level = try MLKEM(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLKEMPublicKey.fromBytes(level, bytes)
    }
}

extension MLKEMPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension MLKEMPublicKey: CustomStringConvertible {
    public var description: String {
        switch levelValue {
        case .mlkem512:
            return "MLKEM512PublicKey(\(refHexShort()))"
        case .mlkem768:
            return "MLKEM768PublicKey(\(refHexShort()))"
        case .mlkem1024:
            return "MLKEM1024PublicKey(\(refHexShort()))"
        }
    }
}

private func postQuantumError(_ error: any Swift.Error) -> BCComponentsError {
    if let error = error as? BCComponentsError {
        return error
    }
    return .postQuantum(String(describing: error))
}
