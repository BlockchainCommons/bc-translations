import BCTags
import DCBOR
import Foundation

public struct MLKEMCiphertext: Equatable, Hashable, Sendable {
    private let levelValue: MLKEM
    private let ciphertextData: Data

    var bytesArray: [UInt8] {
        Array(ciphertextData)
    }

    public static func fromBytes(
        _ level: MLKEM,
        _ bytes: some DataProtocol
    ) throws(BCComponentsError) -> MLKEMCiphertext {
        try MLKEMCiphertext(level: level, bytes: Data(bytes))
    }

    private init(level: MLKEM, bytes: Data) throws(BCComponentsError) {
        try requireLength(
            bytes,
            expected: level.ciphertextSize(),
            name: "MLKEM ciphertext"
        )
        self.levelValue = level
        self.ciphertextData = bytes
    }

    public func level() -> MLKEM {
        levelValue
    }

    public func size() -> Int {
        levelValue.ciphertextSize()
    }

    public func asBytes() -> Data {
        ciphertextData
    }
}

extension MLKEMCiphertext: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.mlkemCiphertext]
    }

    public var untaggedCBOR: CBOR {
        .array([levelValue.cbor, .bytes(ciphertextData)])
    }
}

extension MLKEMCiphertext: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .array(let elements) = untaggedCBOR else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMCiphertext",
                reason: "must be an array"
            )
        }
        guard elements.count == 2 else {
            throw BCComponentsError.invalidData(
                dataType: "MLKEMCiphertext",
                reason: "must have two elements"
            )
        }
        let level = try MLKEM(cbor: elements[0])
        let bytes = try byteString(elements[1])
        self = try MLKEMCiphertext.fromBytes(level, bytes)
    }
}
