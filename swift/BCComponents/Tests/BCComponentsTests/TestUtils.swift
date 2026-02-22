import Foundation
import XCTest

func hexData(_ hex: String, file: StaticString = #filePath, line: UInt = #line) -> Data {
    let cleaned = hex.trimmingCharacters(in: .whitespacesAndNewlines)
    XCTAssertEqual(cleaned.count % 2, 0, file: file, line: line)
    var data = Data(capacity: cleaned.count / 2)
    var index = cleaned.startIndex
    while index < cleaned.endIndex {
        let next = cleaned.index(index, offsetBy: 2)
        let byte = cleaned[index..<next]
        guard let value = UInt8(byte, radix: 16) else {
            XCTFail("invalid hex", file: file, line: line)
            return Data()
        }
        data.append(value)
        index = next
    }
    return data
}

func hexString(_ data: Data) -> String {
    data.map { String(format: "%02x", $0) }.joined()
}
