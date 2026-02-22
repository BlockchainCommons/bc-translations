import BCTags
import DCBOR
import Foundation

public struct MLDSASignature: Equatable, Hashable, Sendable {
    private let levelValue: MLDSA
    private let signatureData: Data

    var bytesArray: [UInt8] {
        Array(signatureData)
    }

    public static func fromBytes(
        _ level: MLDSA,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLDSASignature {
        try MLDSASignature(level: level, bytes: Data(bytes))
    }

    private init(level: MLDSA, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.signatureSize(),
            name: "MLDSA signature"
        )
        self.levelValue = level
        self.signatureData = bytes
    }

    public var level: MLDSA {
        levelValue
    }

    public var size: Int {
        levelValue.signatureSize()
    }

    public var data: Data {
        signatureData
    }
}

extension MLDSASignature: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mldsaSignature]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(signatureData)])
    }
}

extension MLDSASignature: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSASignature",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLDSASignature",
                reason: "must have two elements"
            )
        }
        let level = try MLDSA(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLDSASignature.fromBytes(level, bytes)
    }
}
