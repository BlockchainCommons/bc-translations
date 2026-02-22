import BCUR
import DCBOR
import Foundation

func parseHex(_ hex: String) throws(BCComponentsError) -> Data {
    let cleaned = hex.trimmingCharacters(in: .whitespacesAndNewlines)
    guard cleaned.count.isMultiple(of: 2) else {
        throw .invalidData(dataType: "hex", reason: "odd-length hex string")
    }

    var data = Data(capacity: cleaned.count / 2)
    var index = cleaned.startIndex
    while index < cleaned.endIndex {
        let next = cleaned.index(index, offsetBy: 2)
        let byte = cleaned[index..<next]
        guard let value = UInt8(byte, radix: 16) else {
            throw .invalidData(dataType: "hex", reason: "invalid hex digits")
        }
        data.append(value)
        index = next
    }
    return data
}

func hexEncode(_ data: some DataProtocol) -> String {
    data.reduce(into: "") { result, byte in
        result += String(format: "%02x", byte)
    }
}

func requireLength(
    _ data: some DataProtocol,
    expected: Int,
    name: String
) throws(BCComponentsError) {
    if data.count != expected {
        throw .invalidSize(dataType: name, expected: expected, actual: data.count)
    }
}

func byteString(_ cbor: CBOR) throws(BCComponentsError) -> Data {
    guard case .bytes(let data) = cbor else {
        throw .cbor(.wrongType)
    }
    return data
}

func textString(_ cbor: CBOR) throws(BCComponentsError) -> String {
    guard case .text(let text) = cbor else {
        throw .cbor(.wrongType)
    }
    return text
}

func asUppercaseIdentifier(_ data: [UInt8], prefix: String?) -> String {
    let words = Bytewords.identifier(data).uppercased()
    if let prefix {
        return "\(prefix) \(words)"
    }
    return words
}

func asUppercaseBytemoji(_ data: [UInt8], prefix: String?) -> String {
    let emoji = Bytewords.bytemojiIdentifier(data).uppercased()
    if let prefix {
        return "\(prefix) \(emoji)"
    }
    return emoji
}
