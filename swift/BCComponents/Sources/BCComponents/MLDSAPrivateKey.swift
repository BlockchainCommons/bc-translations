import BCTags
import DCBOR
import Foundation
import SwiftDilithium

public struct MLDSAPrivateKey: Equatable, Hashable, Sendable {
    private let levelValue: MLDSA
    private let keyData: Data

    public static func fromBytes(
        _ level: MLDSA,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLDSAPrivateKey {
        try MLDSAPrivateKey(level: level, bytes: Data(bytes))
    }

    private init(level: MLDSA, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.privateKeySize(),
            name: "MLDSA private key"
        )
        do {
            _ = try SecretKey(keyBytes: Array(bytes))
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
        levelValue.privateKeySize()
    }

    public func asBytes() -> Data {
        keyData
    }

    public func sign(_ message: some DataProtocol) -> MLDSASignature {
        let key = try! SecretKey(keyBytes: Array(keyData))
        let signatureData = key.Sign(message: Array(message), randomize: true)
        return try! MLDSASignature.fromBytes(levelValue, Data(signatureData))
    }
}

extension MLDSAPrivateKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mldsaPrivateKey]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(keyData)])
    }
}

extension MLDSAPrivateKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSAPrivateKey",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSAPrivateKey",
                reason: "must have two elements"
            )
        }
        let level = try MLDSA(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLDSAPrivateKey.fromBytes(level, bytes)
    }
}

extension MLDSAPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension MLDSAPrivateKey: CustomStringConvertible {
    public var description: String {
        switch levelValue {
        case .mldsa44:
            return "MLDSA44PrivateKey(\(refHexShort()))"
        case .mldsa65:
            return "MLDSA65PrivateKey(\(refHexShort()))"
        case .mldsa87:
            return "MLDSA87PrivateKey(\(refHexShort()))"
        }
    }
}

private func postQuantumError(_ error: any Swift.Error) -> BCComponentsError {
    if let error = error as? BCComponentsError {
        return error
    }
    return .postQuantum(String(describing: error))
}
