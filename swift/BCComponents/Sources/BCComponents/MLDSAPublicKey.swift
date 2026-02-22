import BCTags
import DCBOR
import Foundation
import SwiftDilithium

public struct MLDSAPublicKey: Equatable, Hashable, Sendable {
    private let levelValue: MLDSA
    private let keyData: Data

    public static func fromBytes(
        _ level: MLDSA,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLDSAPublicKey {
        try MLDSAPublicKey(level: level, bytes: Data(bytes))
    }

    private init(level: MLDSA, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.publicKeySize(),
            name: "MLDSA public key"
        )
        do {
            _ = try PublicKey(keyBytes: Array(bytes))
        } catch {
            throw postQuantumError(error)
        }
        self.levelValue = level
        self.keyData = bytes
    }

    public func level() -> MLDSA {
        levelValue
    }

    public func size() -> Int {
        levelValue.publicKeySize()
    }

    public func asBytes() -> Data {
        keyData
    }

    public func verify(
        _ signature: MLDSASignature,
        _ message: some DataProtocol
    ) throws(BCComponentsError) -> Bool {
        guard signature.level() == levelValue else {
            throw .levelMismatch
        }
        do {
            let key = try PublicKey(keyBytes: Array(keyData))
            return key.Verify(
                message: Array(message),
                signature: signature.bytesArray
            )
        } catch {
            throw postQuantumError(error)
        }
    }
}

extension MLDSAPublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mldsaPublicKey]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(keyData)])
    }
}

extension MLDSAPublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSAPublicKey",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSAPublicKey",
                reason: "must have two elements"
            )
        }
        let level = try MLDSA(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLDSAPublicKey.fromBytes(level, bytes)
    }
}

extension MLDSAPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension MLDSAPublicKey: CustomStringConvertible {
    public var description: String {
        switch levelValue {
        case .mldsa44:
            return "MLDSA44PublicKey(\(refHexShort()))"
        case .mldsa65:
            return "MLDSA65PublicKey(\(refHexShort()))"
        case .mldsa87:
            return "MLDSA87PublicKey(\(refHexShort()))"
        }
    }
}

private func postQuantumError(_ error: any Swift.Error) -> BCComponentsError {
    if let error = error as? BCComponentsError {
        return error
    }
    return .postQuantum(String(describing: error))
}
